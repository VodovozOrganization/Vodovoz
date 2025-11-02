using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
using TrueMark.Contracts;
using TrueMark.Contracts.Documents;
using TrueMarkWorker.Enums;
using TrueMarkWorker.Factories;
using TrueMarkWorker.Options;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Infrastructure;
using Vodovoz.Zabbix.Sender;
using VodovozBusiness.Controllers;
using Order = Vodovoz.Domain.Orders.Order;

namespace TrueMarkWorker
{
	public partial class TrueMarkWorker : TimerBackgroundServiceBase
	{
		private readonly IOptions<TrueMarkWorkerOptions> _options;
		private readonly ILogger<TrueMarkWorker> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		private bool _workInProgress;
		private const int _createDocumentDelaySec = 5;

		public TrueMarkWorker(
			ILogger<TrueMarkWorker> logger,
			IServiceScopeFactory serviceScopeFactory,
			IOptions<TrueMarkWorkerOptions> options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));

			Interval = options.Value.Interval;
		}

		protected override void OnStartService()
		{
			_logger.LogInformation(
				"Воркер {Worker} запущен в: {TransferStartTime}",
				nameof(TrueMarkWorker),
				DateTimeOffset.Now);

			base.OnStartService();
		}

		protected override void OnStopService()
		{
			_logger.LogInformation(
				"Воркер {Worker} завершил работу в: {StopTime}",
				nameof(TrueMarkWorker),
				DateTimeOffset.Now);

			base.OnStopService();
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken stoppingToken)
		{

			if(_workInProgress)
			{
				return;
			}

			_workInProgress = true;

			try
			{
				using var scope = _serviceScopeFactory.CreateScope();

				await WorkAsync(scope.ServiceProvider, stoppingToken);

				var zabbixSender = scope.ServiceProvider.GetRequiredService<IZabbixSender>();

				await zabbixSender.SendIsHealthyAsync(stoppingToken);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при при обработке документов {ErrorDateTime}",
					DateTimeOffset.Now);
			}
			finally
			{
				_workInProgress = false;
			}

			_logger.LogInformation(
				"Воркер {WorkerName} ожидает '{DelayTime}' перед следующим запуском", nameof(TrueMarkWorker), Interval);

			await Task.CompletedTask;
		}

		private async Task WorkAsync(IServiceProvider serviceProvider, CancellationToken stoppingToken)
		{
			try
			{
				var startDate = DateTime.Today.AddDays(-3);

				var unitOfWorkFactory = serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
				using var uow = unitOfWorkFactory.CreateWithoutRoot("Отправка документов в ЧЗ");
				var organizationRepository = serviceProvider.GetRequiredService<IOrganizationRepository>();
				var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
				var orderRepository = serviceProvider.GetRequiredService<IOrderRepository>();
				var edoAccountController = serviceProvider.GetRequiredService<ICounterpartyEdoAccountController>();

				foreach(var certificate in _options.Value.OrganizationCertificates)
				{
					var organization = organizationRepository.GetOrganizationByTaxcomEdoAccountId(uow, certificate.EdxClientId);
					_logger.LogInformation("Запускаем необходимые транзакции для организации {OrganizationId}. " +
										   "Отпечаток сертификата {CertificateThumbPrint}, " +
										   "ИНН {Inn}, " +
										   "Id кабинета ЭДО {OrganizationCertificateEdxClientId}",
						organization.Id, certificate.CertificateThumbPrint, certificate.Inn, certificate.EdxClientId);

					await ProcessOrganizationDocuments(
						uow, httpClientFactory, orderRepository, edoAccountController, startDate, organization, certificate, stoppingToken);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, $"Ошибка при запуске");
			}
		}

		private async Task ProcessOrganizationDocuments(
			IUnitOfWork uow,
			IHttpClientFactory httpClientFactory,
			IOrderRepository orderRepository,
			ICounterpartyEdoAccountController edoAccountController,
			DateTime startDate,
			Organization organization,
			OrganizationCertificate certificate,
			CancellationToken cancellationToken)
		{
			try
			{
				if(organization is null)
				{
					_logger.LogError("Не найдена организация по edxClientId {OrganizationCertificateEdxClientId}", certificate.EdxClientId);
					throw new InvalidOperationException("В организации не настроено соответствие кабинета ЭДО");
				}

				var token = await Login(httpClientFactory, certificate.CertificateThumbPrint, certificate.Inn, cancellationToken);

				_logger.LogInformation("Получили токен авторизации: {AuthorizationToken} ", token);

				var externalHttpClient = GetHttpClient(httpClientFactory, token, _options.Value.ExternalTrueMarkBaseUrl);

				await ProcessNewOrders(
					uow,
					orderRepository,
					httpClientFactory,
					edoAccountController,
					externalHttpClient,
					startDate,
					organization,
					certificate,
					cancellationToken);

				await ProcessOldOrdersWithErrors(uow, orderRepository, httpClientFactory, externalHttpClient, startDate, organization, certificate, cancellationToken);

				await ProcessCancellationDocuments(uow, orderRepository, httpClientFactory, externalHttpClient, organization, certificate, cancellationToken);

			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе получения заказов для формирования документа вывода из оборота");
			}
		}

		private async Task ProcessNewOrders(
			IUnitOfWork uow,
			IOrderRepository orderRepository,
			IHttpClientFactory httpClientFactory,
			ICounterpartyEdoAccountController edoAccountController,
			HttpClient httpClient,
			DateTime startDate,
			Organization organization,
			OrganizationCertificate certificate,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем заказы для организации {OrganizationId}, " +
								   "отпечаток сертификата {OrganizationCertificateThumbPrint}, " +
								   "ИНН {Inn}, ",
								   "код личного кабинета {OrganizationTaxcomEdoAccountId}, по которым надо осуществить вывод из оборота",
								   organization.Id, certificate.CertificateThumbPrint, certificate.Inn, organization.TaxcomEdoSettings.EdoAccount);

			var orders = orderRepository.GetOrdersForTrueMark(uow, startDate, organization.Id);

			_logger.LogInformation("Всего заказов для формирования выводов из оборота и отправки: {OrdersCount}", orders.Count);

			if(!orders.Any())
			{
				return;
			}

			try
			{
				await CheckAndSaveRegistrationInTrueApi(orders, httpClientFactory, uow, edoAccountController, cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе проверки региатрации контрагента № и его отправки");
			}

			foreach(var order in orders)
			{
				_logger.LogInformation("Создаем вывод из оборота по заказу №{OrderId} для организации {OrganizationId}, " +
									   "отпечаток сертификата {OrganizationCertificateThumbPrint}, ИНН {Inn}" +
									   "код личного кабинета {OrganizationTaxcomEdoAccountId}",
					order.Id, organization.Id, certificate.CertificateThumbPrint, certificate.Inn, organization.TaxcomEdoSettings.EdoAccount);

				try
				{
					var productDocumentFactory = CreateProductDocumentFactory(organization.INN, order);

					var trueMarkApiDocument =
						await CreateAndSendDocument(httpClientFactory, httpClient, TrueMarkDocumentType.LK_GTIN_RECEIPT, productDocumentFactory, certificate, cancellationToken);

					trueMarkApiDocument.Order = order;
					trueMarkApiDocument.Organization = organization;
					trueMarkApiDocument.Type = TrueMarkDocument.TrueMarkDocumentType.Withdrawal;

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

		private async Task ProcessCancellationDocuments(IUnitOfWork uow, IOrderRepository orderRepository, IHttpClientFactory httpClientFactory, HttpClient httpClient, Organization organization,
			OrganizationCertificate certificate, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем список для отмены вывода из оборота для организации {OrganizationId}", organization.Id);

			var startDate = DateTime.Now.AddYears(-1);

			var documentsForCancellation = orderRepository.GetOrdersForCancellationInTrueMark(uow, startDate, organization.Id);

			_logger.LogInformation("Всего документов для отмены вывода из оборота: {ErrorOrdersCount}", documentsForCancellation.Count);

			if(!documentsForCancellation.Any())
			{
				return;
			}

			foreach(var doc in documentsForCancellation)
			{
				_logger.LogInformation("Отмена вывода из оборота по заказу №{OrderId}", doc.OrderId);

				try
				{
					var documentFactory = new CancellationDocumentFactory(organization.INN, doc.DocGuid.ToString());

					var trueMarkApiDocument =
						await CreateAndSendDocument(httpClientFactory, httpClient, TrueMarkDocumentType.LK_GTIN_RECEIPT_CANCEL, documentFactory, certificate, cancellationToken);

					trueMarkApiDocument.Order = new Order { Id = doc.OrderId };
					trueMarkApiDocument.Type = TrueMarkDocument.TrueMarkDocumentType.WithdrawalCancellation;

					if(!trueMarkApiDocument.IsSuccess)
					{
						_logger.LogError("{ErrorMessage}", trueMarkApiDocument.ErrorMessage);
					}

					var actions = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
						.Where(x => x.Order.Id == doc.OrderId)
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
					_logger.LogError(e, "Ошибка в процессе формирования документа отмены вывода из оборота для заказа №{OrderId} и его отправки", doc.OrderId);
				}
			}
		}

		private async Task ProcessOldOrdersWithErrors(
			IUnitOfWork uow,
			IOrderRepository orderRepository,
			IHttpClientFactory httpClientFactory,
			HttpClient httpClient,
			DateTime startDate,
			Organization organization,
			OrganizationCertificate certificate,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем заказы с ошибками вывода из оборота для организации {OrganizationId}", organization.Id);

			var errorOrders = orderRepository.GetOrdersWithSendErrorsForTrueMarkApi(uow, startDate, organization.Id);

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
					var savedDocument = uow.GetAll<TrueMarkDocument>().SingleOrDefault(
						d => d.Order.Id == order.Id
							&& d.Type == TrueMarkDocument.TrueMarkDocumentType.Withdrawal);

					TrueMarkDocument recievedDocument;

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

					recievedDocument = await CreateAndSendDocument(httpClientFactory, httpClient, TrueMarkDocumentType.LK_GTIN_RECEIPT, productDocumentFactory, certificate, cancellationToken);
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

		private async Task<TrueMarkDocument> CreateAndSendDocument(IHttpClientFactory httpClientFactory, HttpClient httpClient, TrueMarkDocumentType trueMarkDocumentType, IDocumentFactory productDocumentFactory,
			OrganizationCertificate certificate, CancellationToken cancellationToken)
		{
			var documentCreateUrl = "v3/true-api/lk/documents/create?pg=water";

			var document = productDocumentFactory.CreateDocument();

			var internalHttpClient = GetHttpClient(httpClientFactory, _options.Value.AuthorizationToken, _options.Value.InternalTrueMarkApiBaseUrl);
			var signEndPoint = $"Sign?data={document}&&isDeatchedSign=true&&certificateThumbPrint={certificate.CertificateThumbPrint}&&inn={certificate.Inn}";
			var signResponse = await internalHttpClient.GetStreamAsync(signEndPoint, cancellationToken);
			var sign = await JsonSerializer.DeserializeAsync<byte[]>(signResponse);
			var documentBytes = Encoding.UTF8.GetBytes(document);

			var gtinReceiptDocument = new GtinReceiptDocumentDto
			{
				DocumentFormat = "MANUAL",
				Type = trueMarkDocumentType.ToString(),
				ProductDocument = Convert.ToBase64String(documentBytes),
				Signature = Convert.ToBase64String(sign)
			};

			var serializedDocument = JsonSerializer.Serialize(gtinReceiptDocument);
			var documentContent = new StringContent(serializedDocument, Encoding.UTF8, "application/json");

			_logger.LogInformation("Отправка: сертификат {OrganizationCertificateThumbPrint}, ИНН {Inn} токен авторизации {AuthorizationToken}",
				certificate.CertificateThumbPrint, certificate.Inn, httpClient.DefaultRequestHeaders.Authorization);

			var documentResponse = await httpClient.PostAsync(documentCreateUrl, documentContent, cancellationToken);

			if(!documentResponse.IsSuccessStatusCode)
			{
				_logger.LogError("Ошибка: сертификат {OrganizationCertificateThumbPrint}, ИНН {Inn} токен авторизации {AuthorizationToken}",
				certificate.CertificateThumbPrint, certificate.Inn, httpClient.DefaultRequestHeaders.Authorization);


				return new TrueMarkDocument
				{
					IsSuccess = false,
					ErrorMessage = $"Ошибка при создании документа. Error status code: {documentResponse.StatusCode}. Response phrase: {documentResponse.ReasonPhrase}"
				};
			}

			var documentId = await documentResponse.Content.ReadAsStringAsync(cancellationToken);

			return await RecieveDocument(httpClient, documentId, cancellationToken);
		}

		private async Task<TrueMarkDocument> RecieveDocument(
			HttpClient httpClient,
			string documentId,
			CancellationToken cancellationToken)
		{
			var resultInfoUrl = $"v4/true-api/doc/{documentId}/info";

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
				return new TrueMarkDocument
				{
					Guid = new Guid(documentId),
					IsSuccess = false,
					ErrorMessage = $"Не удалось получить результат создания документа. Error status code: {resultInfoResponse.StatusCode}. Response phrase: {resultInfoResponse.ReasonPhrase}"
				};
			}

			var resultInfoResponseBody = await resultInfoResponse.Content.ReadAsStreamAsync(cancellationToken);
			createdDocumentInfo = (await JsonSerializer.DeserializeAsync<IEnumerable<CreatedDocumentInfoDto>>(resultInfoResponseBody))
				.FirstOrDefault();

			if(createdDocumentInfo == null)
			{
				return new TrueMarkDocument
				{
					Guid = new Guid(documentId),
					IsSuccess = false,
					ErrorMessage = "Не удалось получить информацию о статусе создания документа(возможных ошибках)"
				};
			}

			if(createdDocumentInfo.Errors != null && createdDocumentInfo.Errors.Any())
			{
				return new TrueMarkDocument
				{
					Guid = new Guid(documentId),
					IsSuccess = false,
					ErrorMessage = "Сформировано с ошибками: " + string.Join("; ", createdDocumentInfo.Errors)
				};
			}

			return new TrueMarkDocument
			{
				Guid = new Guid(documentId),
				IsSuccess = true
			};
		}

		private async Task CheckAndSaveRegistrationInTrueApi(
			IList<Order> orders,
			IHttpClientFactory httpClientFactory,
			IUnitOfWork uow,
			ICounterpartyEdoAccountController edoAccountController,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation("Проверям регистрацию клиентов в Честном Знаке");

			var notRegisteredInns =
				orders.Where(o => o.Client.RegistrationInChestnyZnakStatus != RegistrationInChestnyZnakStatus.Registered)
					.Select(o => o.Client.INN).ToList();

			if(!notRegisteredInns.Any())
			{
				return;
			}

			// Ограничение в 100 инн
			var splitedNotRegisteredInns = notRegisteredInns
				.Select((x, i) => new { Index = i, Value = x })
				.GroupBy(x => x.Index / 100)
				.Select(x => x.Select(v => v.Value).ToList())
				.ToList();

			foreach(var inns in splitedNotRegisteredInns)
			{
				var registrations = await GetParticipantsRegistrations(httpClientFactory, inns, cancellationToken);

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

						CheckAndRemoveOrderFromProcessingList(edoAccountController, orders, orderForUpdate);
					}
				}
			}

			uow.Commit();
		}

		private async Task<IList<ParticipantRegistrationDto>> GetParticipantsRegistrations(IHttpClientFactory httpClientFactory, IList<string> notRegisteredInns, CancellationToken cancellationToken)
		{
			var serializedNotRegisteredInns = JsonSerializer.Serialize(notRegisteredInns);
			var content = new StringContent(serializedNotRegisteredInns, Encoding.UTF8, "application/json");

			var internalHttpClient = GetHttpClient(httpClientFactory, _options.Value.AuthorizationToken, _options.Value.InternalTrueMarkApiBaseUrl);

			var response = await internalHttpClient.PostAsync("Participants", content, cancellationToken);

			if(response.IsSuccessStatusCode)
			{
				var responseBody = await response.Content.ReadAsStreamAsync();

				var registrations = await JsonSerializer.DeserializeAsync<IList<ParticipantRegistrationDto>>(responseBody, cancellationToken: cancellationToken);

				return registrations;
			}

			return new List<ParticipantRegistrationDto>();
		}

		private void CheckAndRemoveOrderFromProcessingList(
			ICounterpartyEdoAccountController edoAccountController,
			IList<Order> orders,
			Order order)
		{
			if(order.Client == null)
			{
				return;
			}

			var edoAccount =
				edoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(order.Client, order.Contract.Organization.Id);

			if(edoAccount is { ConsentForEdoStatus: ConsentForEdoStatus.Agree })
			{
				orders.Remove(order);
			}
		}

		private async Task<string> Login(IHttpClientFactory httpClientFactory, string certificateThumbPrint, string inn, CancellationToken cancellationToken)
		{
			var internalHttpClient = GetHttpClient(httpClientFactory, _options.Value.AuthorizationToken, _options.Value.InternalTrueMarkApiBaseUrl);
			var endPoint = $"Login?certificateThumbPrint={certificateThumbPrint}&&inn={inn}";
			var token = await internalHttpClient.GetStringAsync(endPoint, cancellationToken);

			return token;
		}

		private HttpClient GetHttpClient(IHttpClientFactory httpClientFactory, string token, string baseAddress)
		{
			var httpClient = httpClientFactory.CreateClient();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			httpClient.BaseAddress = new Uri(baseAddress);

			return httpClient;
		}
	}
}
