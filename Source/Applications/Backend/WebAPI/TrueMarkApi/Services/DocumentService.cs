using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Dto;
using TrueMarkApi.Dto.Documents;
using TrueMarkApi.Enums;
using TrueMarkApi.Factories;
using TrueMarkApi.Library;
using TrueMarkApi.Models;
using TrueMarkApi.Services.Authorization;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Settings.Edo;
using Order = Vodovoz.Domain.Orders.Order;

namespace TrueMarkApi.Services
{
	public class DocumentService : BackgroundService
	{
		private readonly IAuthorizationService _authorizationService;
		private readonly IEdoSettings _edoSettings;
		private readonly IHttpClientFactory _httpClientClientFactory;
		private readonly IConfigurationSection _apiSection;
		private readonly ILogger<DocumentService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly OrganizationCertificate[] _organizationsCertificates;
		private readonly TrueMarkApiClient _trueMarkApiClient;
		private const int _workerDelaySec = 300;
		private const int _createDocumentDelaySec = 5;

		public DocumentService(
			ILogger<DocumentService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IOrganizationRepository organizationRepository,
			IConfiguration configuration,
			IHttpClientFactory httpClientFactory,
			IAuthorizationService authorizationService,
			IEdoSettings edoSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_apiSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("Api");
			_httpClientClientFactory = httpClientFactory;

			var organizationsCertificateSection = _apiSection.GetSection("OrganizationCertificates");
			_organizationsCertificates = organizationsCertificateSection.Get<OrganizationCertificate[]>();
			_trueMarkApiClient = new TrueMarkApiClient(_edoSettings.TrueMarkApiBaseUrl, _edoSettings.TrueMarkApiToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Служба запущена.");
			await StartWorkingAsync(stoppingToken);
		}

		private async Task StartWorkingAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var startDate = DateTime.Today.AddDays(-_edoSettings.EdoCheckPeriodDays);

					using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
					{
						foreach(var organizationCertificate in _organizationsCertificates)
						{
							var organization = _organizationRepository.GetOrganizationByTaxcomEdoAccountId(uow, organizationCertificate.EdxClientId);
							_logger.LogInformation("Запускаем необходимые транзакции для организации {OrganizationId}. " + 
							                       "Отпечаток сертификата {CertificateThumbPrint}, " +
							                       "Id кабинета ЭДО {OrganizationCertificateEdxClientId}", 
								organization.Id, organizationCertificate.CertificateThumbPrint, organizationCertificate.EdxClientId);

							await ProcessOrganizationDocuments(uow, startDate, organization, organizationCertificate, stoppingToken);
						}
					}

					_logger.LogInformation($"Пауза перед запуском следующих транзакций {_workerDelaySec} сек");
					await Task.Delay(_workerDelaySec * 1000, stoppingToken);
				}
				catch(Exception e)
				{
					_logger.LogError(e, $"Ошибка при запуске");
				}
			}
		}

		private async Task ProcessOrganizationDocuments(IUnitOfWork uow, DateTime startDate, Organization organization,
			OrganizationCertificate organizationCertificate, CancellationToken cancellationToken)
		{
			try
			{
				if(organization is null)
				{
					_logger.LogError("Не найдена организация по edxClientId {OrganizationCertificateEdxClientId}", organizationCertificate.EdxClientId);
					throw new InvalidOperationException("В организации не настроено соответствие кабинета ЭДО");
				}

				var token = await _authorizationService.Login(organizationCertificate.CertificateThumbPrint);

				_logger.LogInformation("Получили токен авторизации: {AuthorizationToken} ", token);

				var httpClient = GetHttpClient(token);

				await ProcessNewOrders(uow, httpClient, startDate, organization, organizationCertificate.CertificateThumbPrint, cancellationToken);

				await ProcessOldOrdersWithErrors(uow, httpClient, startDate, organization, organizationCertificate.CertificateThumbPrint, cancellationToken);

				await ProcessCancellationDocuments(uow, httpClient, organization, organizationCertificate.CertificateThumbPrint, cancellationToken);

			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе получения заказов для формирования документа вывода из оборота");
			}
		}

		private HttpClient GetHttpClient(string token)
		{
			var httpClient = _httpClientClientFactory.CreateClient();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpClient.BaseAddress = new Uri(_apiSection.GetValue<string>("ExternalTrueApiBaseUrl"));
			
			return httpClient;
		}

		private async Task ProcessCancellationDocuments(IUnitOfWork uow, HttpClient httpClient, Organization organization,
			string certificateThumbPrint, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем список для отмены вывода из оборота для организации {OrganizationId}", organization.Id);

			var startDate = DateTime.Now.AddYears(-1);

			var documentsForCancellation = _orderRepository.GetOrdersForCancellationInTrueMark(uow, startDate, organization.Id);

			_logger.LogInformation("Всего документов для отмены вывода из оборота: {ErrorOrdersCount}", documentsForCancellation.Count);

			if(!documentsForCancellation.Any())
			{
				return;
			}

			foreach(var doc in documentsForCancellation)
			{
				_logger.LogInformation("Отмена вывода из оборота по заказу №{OrderId}", doc.Order.Id);

				try
				{
					var documentFactory = new CancellationDocumentFactory(organization.INN, doc.Guid.ToString());

					var trueMarkApiDocument =
						await CreateAndSendDocument(httpClient, TrueMarkDocumentType.LK_GTIN_RECEIPT_CANCEL, documentFactory, certificateThumbPrint, cancellationToken);

					trueMarkApiDocument.Order = doc.Order;
					trueMarkApiDocument.Type = TrueMarkApiDocument.TrueMarkApiDocumentType.WithdrawalCancellation;

					if(!trueMarkApiDocument.IsSuccess)
					{
						_logger.LogError("{ErrorMessage}", trueMarkApiDocument.ErrorMessage);
					}

					var actions = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
						.Where(x => x.Order.Id == doc.Order.Id)
						.FirstOrDefault();

					if(actions != null && actions.IsNeedToCancelTrueMarkDocument)
					{
						actions.IsNeedToCancelTrueMarkDocument = false;
					}

					uow.Save(trueMarkApiDocument);
					uow.Commit();

				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка в процессе формирования документа отмены вывода из оборота для заказа №{OrderId} и его отправки", doc.Order.Id);
				}
			}
		}

		private async Task ProcessNewOrders(IUnitOfWork uow, HttpClient httpClient, DateTime startDate, Organization organization,
			string organizationCertificateThumbPrint, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем заказы для организации {OrganizationId}, " +
			                       "отпечаток сертификата {OrganizationCertificateThumbPrint}, " +
			                       "код личного кабинета {OrganizationTaxcomEdoAccountId}, по которым надо осуществить вывод из оборота", 
				organization.Id, organizationCertificateThumbPrint, organization.TaxcomEdoAccountId);

			var orders = _orderRepository.GetOrdersForTrueMarkApi(uow, startDate, organization.Id);

			_logger.LogInformation("Всего заказов для формирования выводов из оборота и отправки: {OrdersCount}", orders.Count);

			if(!orders.Any())
			{
				return;
			}

			await CheckAndSaveRegistrationInTrueApi(orders, uow, cancellationToken);

			foreach(var order in orders)
			{
				_logger.LogInformation("Создаем вывод из оборота по заказу №{OrderId} для организации {OrganizationId}, " +
				                       "отпечаток сертификата {OrganizationCertificateThumbPrint}," +
				                       "код личного кабинета {OrganizationTaxcomEdoAccountId}", 
					order.Id, organization.Id, organizationCertificateThumbPrint, organization.TaxcomEdoAccountId);

				try
				{
					var productDocumentFactory = CreateProductDocumentFactory(organization.INN, order);

					var trueMarkApiDocument = await CreateAndSendDocument(httpClient, TrueMarkDocumentType.LK_GTIN_RECEIPT, productDocumentFactory, organizationCertificateThumbPrint, cancellationToken);
					trueMarkApiDocument.Order = order;
					trueMarkApiDocument.Type = TrueMarkApiDocument.TrueMarkApiDocumentType.Withdrawal;

					uow.Save(trueMarkApiDocument);
					uow.Commit();

					if(!trueMarkApiDocument.IsSuccess)
					{
						_logger.LogError("{ErrorMessage}", trueMarkApiDocument.ErrorMessage);
					}
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка в процессе формирования документа вывода из оборота для заказа №{OrderId} и его отправки", order.Id);
				}
			}
		}

		private async Task ProcessOldOrdersWithErrors(IUnitOfWork uow, HttpClient httpClient, DateTime startDate, Organization organization,
			string certificateThumbPrint, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем заказы с ошибками вывода из оборота для организации {OrganizationId}", organization.Id);

			var errorOrders = _orderRepository.GetOrdersWithSendErrorsForTrueMarkApi(uow, startDate, organization.Id);

			_logger.LogInformation("Всего заказов с ошибками вывода из оборота: {ErrorOrdersCount}", errorOrders.Count);

			if(!errorOrders.Any())
			{
				return;
			}

			foreach(var order in errorOrders)
			{
				_logger.LogInformation("Обработка ошибки по заказу №{OrderId}", order.Id);

				try
				{
					var savedDocument = uow.GetAll<TrueMarkApiDocument>().SingleOrDefault(
						d => d.Order.Id == order.Id
							&& d.Type == TrueMarkApiDocument.TrueMarkApiDocumentType.Withdrawal);

					TrueMarkApiDocument recievedDocument;

					if(savedDocument.Guid != null)
					{
						recievedDocument = await RecieveDocument(httpClient, savedDocument.Guid.ToString(), cancellationToken);

						savedDocument.ErrorMessage = recievedDocument.ErrorMessage;
						savedDocument.IsSuccess = recievedDocument.IsSuccess;

						uow.Save(savedDocument);
						uow.Commit();
						
						continue;
					}

					IDocumentFactory productDocumentFactory = order.PaymentType == PaymentType.Barter
						? new ProductDocumentForDonationFactory(organization.INN, order)
						: new ProductDocumentForOwnUseFactory(organization.INN, order);

					recievedDocument = await CreateAndSendDocument(httpClient, TrueMarkDocumentType.LK_GTIN_RECEIPT, productDocumentFactory, certificateThumbPrint, cancellationToken);
					savedDocument.ErrorMessage = recievedDocument.ErrorMessage;
					savedDocument.IsSuccess = recievedDocument.IsSuccess;
					savedDocument.Guid = recievedDocument.Guid;

					uow.Save(savedDocument);
					uow.Commit();

					if(!savedDocument.IsSuccess)
					{
						_logger.LogError("{ErrorMessage}", savedDocument.ErrorMessage);
					}
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка в процессе обновления информации/отправки документа вывода из оборота для заказа №{OrderId}", order.Id);
				}
			}
		}

		private IDocumentFactory CreateProductDocumentFactory(string inn, Order order) => order.PaymentType == PaymentType.Barter 
			? new ProductDocumentForDonationFactory(inn, order) 
			: new ProductDocumentForOwnUseFactory(inn, order);

		private async Task<TrueMarkApiDocument> CreateAndSendDocument(HttpClient httpClient, TrueMarkDocumentType trueMarkDocumentType, IDocumentFactory productDocumentFactory,
			string certificateThumbPrint, CancellationToken cancellationToken)
		{
			var documentCreateUrl = "lk/documents/create?pg=water";

			var document = productDocumentFactory.CreateDocument();

			var signModel = new SignModel(certificateThumbPrint, document, false);

			var gtinReceiptDocument = new GtinReceiptDocumentDto
			{
				DocumentFormat = "MANUAL",
				Type = trueMarkDocumentType.ToString(),
				ProductDocument = document,
				Signature = signModel.Sign()
			};

			var serializedDocument = JsonSerializer.Serialize(gtinReceiptDocument);
			var documentContent = new StringContent(serializedDocument, Encoding.UTF8, "application/json");

			_logger.LogInformation("Отправка: сертификат {OrganizationCertificateThumbPrint}, токен авторизации {AuthorizationToken}", 
				certificateThumbPrint, httpClient.DefaultRequestHeaders.Authorization);

			var documentResponse = await httpClient.PostAsync(documentCreateUrl, documentContent, cancellationToken);

			if(!documentResponse.IsSuccessStatusCode)
			{
				_logger.LogError("Ошибка: сертификат {OrganizationCertificateThumbPrint}, токен авторизации {AuthorizationToken}", 
					certificateThumbPrint, httpClient.DefaultRequestHeaders.Authorization);

				return new TrueMarkApiDocument
				{
					IsSuccess = false,
					ErrorMessage = $"Ошибка при создании документа. Error status code: {documentResponse.StatusCode}. Response phrase: {documentResponse.ReasonPhrase}"
				};
			}

			var documentId = await documentResponse.Content.ReadAsStringAsync(cancellationToken);

			return await RecieveDocument(httpClient, documentId, cancellationToken);
		}

		private async Task<TrueMarkApiDocument> RecieveDocument(HttpClient httpClient, string documentId, CancellationToken cancellationToken)
		{
			var resultInfoUrl = $"doc/{documentId}/info";

			// Делаем паузу перед получением информации по созданному документу
			await Task.Delay(_createDocumentDelaySec * 1000, cancellationToken);

			var resultInfoResponse = await httpClient.GetAsync(resultInfoUrl, cancellationToken);

			if(!resultInfoResponse.IsSuccessStatusCode)
			{
				// Производим ещё несколько попыток получения информации по созданному документу
				for(int i = 0; i < 5; i++)
				{
					_logger.LogWarning("Повторный запрос результатов создания документа {DocumentId}", documentId);
					await Task.Delay(_createDocumentDelaySec * 1000, cancellationToken);
					resultInfoResponse = await httpClient.GetAsync(resultInfoUrl, cancellationToken);

					if(resultInfoResponse.IsSuccessStatusCode)
					{
						break;
					}
				}
			}

			CreatedDocumentInfoDto createdDocumentInfo;

			if(!resultInfoResponse.IsSuccessStatusCode)
			{
				return new TrueMarkApiDocument
				{
					Guid = new Guid(documentId),
					IsSuccess = false,
					ErrorMessage = $"Не удалось получить результат создания документа. Error status code: {resultInfoResponse.StatusCode}. Response phrase: {resultInfoResponse.ReasonPhrase}"
				};
			}

			var resultInfoResponseBody = await resultInfoResponse.Content.ReadAsStreamAsync(cancellationToken);
			createdDocumentInfo = await JsonSerializer.DeserializeAsync<CreatedDocumentInfoDto>(resultInfoResponseBody, cancellationToken: cancellationToken);

			if(createdDocumentInfo == null)
			{
				return new TrueMarkApiDocument
				{
					Guid = new Guid(documentId),
					IsSuccess = false,
					ErrorMessage = "Не удалось получить информацию о статусе создания документа(возможных ошибках)"
				};
			}

			if(createdDocumentInfo.Errors != null && createdDocumentInfo.Errors.Any())
			{
				return new TrueMarkApiDocument
				{
					Guid = new Guid(documentId),
					IsSuccess = false,
					ErrorMessage = "Сформировано с ошибками: " + string.Join("; ", createdDocumentInfo.Errors)
				};
			}

			return new TrueMarkApiDocument
			{
				Guid = new Guid(documentId),
				IsSuccess = true
			};
		}

		private async Task CheckAndSaveRegistrationInTrueApi(IList<Order> orders, IUnitOfWork uow, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Проверям регистрацию клиентов в Четсном Знаке");

			var notRegisteredInns =
				orders.Where(o => o.Client.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.Registered)
					.Select(o => o.Client.INN).ToList();

			if(!notRegisteredInns.Any())
			{
				return;
			}

			var registrations = await _trueMarkApiClient.GetParticipantsRegistrations(_edoSettings.TrueMarkApiParticipantsUri, notRegisteredInns, cancellationToken);

			foreach(var registration in registrations)
			{
				var orderForUpdate = orders.FirstOrDefault(o =>
						o.Client.INN == registration.Inn
						&& (o.Client.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.Registered
							&& registration.IsRegisteredForWater));

				if(orderForUpdate?.Client is Counterparty counterparty)
				{
					_logger.LogInformation("Найдено изменение статуса регистрации клиента {CounterpartyId} в Честном Знаке, сохраняем изменение в базу.", counterparty.Id);

					counterparty.RegistrationInChestnyZnakStatus = RegistrationInChestnyZnakStatus.Registered;

					uow.Save(counterparty);

					CheckAndRemoveOrderFromProcessingList(orders, orderForUpdate);
				}
			}

			uow.Commit();
		}

		private void CheckAndRemoveOrderFromProcessingList(IList<Order> orders, Order order)
		{
			if(order.Client is Counterparty counterparty
			   && counterparty.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
			{
				orders.Remove(order);
			}
		}
	}
}
