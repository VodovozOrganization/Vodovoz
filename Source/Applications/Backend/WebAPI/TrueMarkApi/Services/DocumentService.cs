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
using TrueMarkApi.Dto.Participants;
using TrueMarkApi.Factories;
using TrueMarkApi.Models;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Services;
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
					_logger.LogInformation($"Пауза перед запуском транзакций {_workerDelaySec} сек");
					await Task.Delay(_workerDelaySec * 1000, stoppingToken);
					_logger.LogInformation("Запускаем необходимые транзакции");

					var startDate = DateTime.Today.AddDays(-_edoSettings.EdoCheckPeriodDays);

					var organizationsCertificateSection = _apiSection.GetSection("OrganizationCertificates");
					var organizationsCertificates = organizationsCertificateSection.Get<OrganizationCertificate[]>().ToArray();

					using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
					{
						foreach(var organizationCertificate in organizationsCertificates)
						{
							await WithdrawFromCirculation(uow, startDate, organizationCertificate);
						}
					}
				}
				catch(Exception e)
				{
					_logger.LogError(e, $"Ошибка при запуске");
				}
			}
		}

		private async Task WithdrawFromCirculation(IUnitOfWork uow, DateTime startDate, OrganizationCertificate organizationCertificate)
		{
			try
			{
				var organization = _organizationRepository.GetOrganizationByTaxcomEdoAccountId(uow, organizationCertificate.EdxClientId);
				_logger.LogInformation("Организация получена");

				if(organization is null)
				{
					_logger.LogError($"Не найдена организация по edxClientId {organizationCertificate.EdxClientId}");
					throw new InvalidOperationException("В организации не настроено соответствие кабинета ЭДО");
				}

				_logger.LogInformation("Получаем заказы, по которым надо осуществить вывод из оборота");
				var orders = _orderRepository.GetOrdersForTrueMarkApi(uow, startDate, organization.Id);

				_logger.LogInformation($"Всего заказов для формирования выводов из оборота и отправки: {orders.Count}");

				if(!orders.Any())
				{
					return;
				}

				// На данный момент не проверяем в ЧЗ и не сохраняем в контрагенте
				// await CheckAndSaveRegistrationInTrueApi(orders, uow);

				foreach(var order in orders)
				{
					_logger.LogInformation($"Создаем вывод из оборота по заказу №{order.Id}");

					try
					{
						IProductDocumentFactory productDocumentFactory = order.PaymentType == PaymentType.barter
							? new ProductDocumentForDonationFactory(organization.INN, order)
							: new ProductDocumentForOwnUseFactory(organization.INN, order);

						var trueMarkApiDocument = await CreateAndSendDocument(productDocumentFactory, organizationCertificate.CertificateThumbPrint);
						trueMarkApiDocument.Order = order;
						uow.Save(trueMarkApiDocument);
						uow.Commit();

						if(!trueMarkApiDocument.IsSuccess)
						{
							_logger.LogError(trueMarkApiDocument.ErrorMessage);
						}
					}
					catch(Exception e)
					{
						_logger.LogError(e, $"Ошибка в процессе формирования документа вывода из оборота для заказа №{order.Id} и его отправки");
					}
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе получения заказов для формирования документа вывода из оборота");
			}
		}

		private async Task<TrueMarkApiDocument> CreateAndSendDocument(IProductDocumentFactory productDocumentFactory, string certificateThumbPrint)
		{
			var documentCreateUrl = "lk/documents/create?pg=water";

			var token = await _authorizationService.Login(certificateThumbPrint);

			var httpClient = _httpClientClientFactory.CreateClient();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpClient.BaseAddress = new Uri(_apiSection.GetValue<string>("ExternalTrueApiBaseUrl"));

			var productDocument = productDocumentFactory.CreateProductDocument();

			var signModel = new SignModel(certificateThumbPrint, productDocument, false);

			var gtinReceiptDocument = new GtinReceiptDocumentDto
			{
				DocumentFormat = "MANUAL",
				Type = "LK_GTIN_RECEIPT",
				ProductDocument = productDocument,
				Signature = signModel.Sign()
			};

			var serializedDocument = JsonSerializer.Serialize(gtinReceiptDocument);
			var documentContent = new StringContent(serializedDocument, Encoding.UTF8, "application/json");
			var documentResponse = await httpClient.PostAsync(documentCreateUrl, documentContent);

			if(!documentResponse.IsSuccessStatusCode)
			{
				return new TrueMarkApiDocument
				{
					IsSuccess = false,
					ErrorMessage = $"Error status code: {documentResponse.StatusCode}. Response phrase: {documentResponse.ReasonPhrase}"
				};
			}

			var documentId = await documentResponse.Content.ReadAsStringAsync();

			var resultInfoUrl = $"doc/{documentId}/info";

			// Делаем паузу перед получением информации по созданному документу
			await Task.Delay(_createDocumentDelaySec * 1000);

			var resultInfoResponse = await httpClient.GetAsync(resultInfoUrl);

			if(!resultInfoResponse.IsSuccessStatusCode)
			{
				// Производим ещё несколько попыток получения информации по созданному документу
				for(int i = 0; i < 5; i++)
				{
					_logger.LogWarning($"Повторный запрос результатов создания документа {documentId}");
					await Task.Delay(_createDocumentDelaySec * 1000);
					resultInfoResponse = await httpClient.GetAsync(resultInfoUrl);

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

			var resultInfoResponseBody = await resultInfoResponse.Content.ReadAsStreamAsync();
			createdDocumentInfo = await JsonSerializer.DeserializeAsync<CreatedDocumentInfoDto>(resultInfoResponseBody);

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

		private async Task CheckAndSaveRegistrationInTrueApi(IList<Order> orders, IUnitOfWork uow, string certificateThumbPrint)
		{
			var url = "participants";

			var token = await _authorizationService.Login(certificateThumbPrint);

			var httpClient = _httpClientClientFactory.CreateClient();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpClient.BaseAddress = new Uri(_apiSection.GetValue<string>("ParticipantsUrl"));

			var notRegisteredInns =
				orders.Where(o => o.Client.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.Registered)
					.Select(o => o.Client.INN).ToList();

			var serializedNotRegisteredInns = JsonSerializer.Serialize(notRegisteredInns);
			var content = new StringContent(serializedNotRegisteredInns, Encoding.UTF8, "application/json");
			var response = await httpClient.PostAsync(url, content);

			if(response.IsSuccessStatusCode)
			{
				var responseBody = await response.Content.ReadAsStreamAsync();

				var registrations = await JsonSerializer.DeserializeAsync<IList<ParticipantRegistrationDto>>(responseBody);

				foreach(var registration in registrations)
				{
					var orderForUpdate = orders.FirstOrDefault(o =>
							o.Client.INN == registration.Inn
							&& (o.Client.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.Registered
								&& registration.IsRegisteredForWater));

					if(orderForUpdate?.Client is Counterparty counterparty)
					{
						counterparty.RegistrationInChestnyZnakStatus = RegistrationInChestnyZnakStatus.Registered;
						uow.Save(counterparty);
						CheckAndRemoveOrder(orders, orderForUpdate);
					}
				}

				uow.Commit();
			}
		}

		private void CheckAndRemoveOrder(IList<Order> orders, Order order)
		{
			if(order.Client is Counterparty counterparty
			   && counterparty.PersonType == PersonType.legal
			   && counterparty.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds
			   && order.PaymentType == PaymentType.cashless
			   && counterparty.ConsentForEdoStatus == ConsentForEdoStatus.Agree
			   && (counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.InProcess
				   || counterparty.RegistrationInChestnyZnakStatus == RegistrationInChestnyZnakStatus.Registered))
			{
				orders.Remove(order);
			}
		}
	}
}
