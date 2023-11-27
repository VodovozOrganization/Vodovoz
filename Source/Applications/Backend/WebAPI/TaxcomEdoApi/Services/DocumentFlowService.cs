using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Report;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Taxcom.Client.Api;
using Taxcom.Client.Api.Converters;
using Taxcom.Client.Api.Entity;
using Taxcom.Client.Api.Entity.DocFlow;
using TaxcomEdoApi.Factories;
using TaxcomEdoApi.HealthChecks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Parameters;
using Vodovoz.Specifications.Orders.EdoContainers;
using Vodovoz.Tools.Orders;
using Type = Vodovoz.Domain.Orders.Documents.Type;

namespace TaxcomEdoApi.Services
{
	public class DocumentFlowService : BackgroundService
	{
		private readonly ILogger<DocumentFlowService> _logger;
		private readonly TaxcomApi _taxcomApi;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IParametersProvider _parametersProvider;
		private readonly IOrderRepository _orderRepository;
		private readonly IGenericRepository<EdoContainer> _edoContainersRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IConfigurationSection _apiSection;
		private readonly EdoUpdFactory _edoUpdFactory;
		private readonly EdoBillFactory _edoBillFactory;
		private readonly EdoContainerMainDocumentIdParser _edoContainerMainDocumentIdParser;
		private readonly X509Certificate2 _certificate;
		private readonly PrintableDocumentSaver _printableDocumentSaver;
		private readonly TaxcomEdoApiHealthCheck _taxcomEdoApiHealthCheck;
		private const int _delaySec = 90;

		private long? _lastEventIngoingDocumentsTimeStamp;
		private long? _lastEventOutgoingDocumentsTimeStamp;

		public DocumentFlowService(
			ILogger<DocumentFlowService> logger,
			TaxcomApi taxcomApi,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IParametersProvider parametersProvider,
			IOrderRepository orderRepository,
			IGenericRepository<EdoContainer> edoContainersRepository,
			IOrganizationRepository organizationRepository,
			EdoUpdFactory edoUpdFactory,
			EdoBillFactory edoBillFactory,
			EdoContainerMainDocumentIdParser edoContainerMainDocumentIdParser,
			X509Certificate2 certificate,
			PrintableDocumentSaver printableDocumentSaver,
			TaxcomEdoApiHealthCheck taxcomEdoApiHealthCheck)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApi = taxcomApi ?? throw new ArgumentNullException(nameof(taxcomApi));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_edoContainersRepository = edoContainersRepository ?? throw new ArgumentNullException(nameof(edoContainersRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_edoUpdFactory = edoUpdFactory ?? throw new ArgumentNullException(nameof(edoUpdFactory));
			_edoBillFactory = edoBillFactory ?? throw new ArgumentNullException(nameof(edoBillFactory));
			_edoContainerMainDocumentIdParser =
				edoContainerMainDocumentIdParser ?? throw new ArgumentNullException(nameof(edoContainerMainDocumentIdParser));
			_certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
			_printableDocumentSaver = printableDocumentSaver;
			_taxcomEdoApiHealthCheck = taxcomEdoApiHealthCheck ?? throw new ArgumentNullException(nameof(taxcomEdoApiHealthCheck));
			_apiSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("Api");
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Процесс электронного документооборота запущен");
			_lastEventIngoingDocumentsTimeStamp = _parametersProvider.GetValue<long>("last_event_ingoing_documents_timestamp");
			_lastEventOutgoingDocumentsTimeStamp = _parametersProvider.GetValue<long>("last_event_outgoing_documents_timestamp");
			await StartWorkingAsync(stoppingToken);
		}

		private async Task StartWorkingAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				await DelayAsync(stoppingToken);

				var startDate = DateTime.Today.AddMonths(-1);

				using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
				{
					_taxcomEdoApiHealthCheck.IsHealthy = await CreateAndSendUpd(uow, startDate);
					await CreateAndSendBills(uow);
					await ProcessOutgoingDocuments(uow);
					await ProcessIngoingDocuments(uow);
				}
			}
		}

		private Task<bool> CreateAndSendUpd(IUnitOfWork uow, DateTime startDate)
		{
			var isHealthy = true;

			try
			{
				var edoAccountId = _apiSection.GetValue<string>("EdxClientId");
				var organization = _organizationRepository.GetOrganizationByTaxcomEdoAccountId(uow, edoAccountId);

				if(organization is null)
				{
					_logger.LogError("Не найдена организация по edxClientId {EdoAccountId}", edoAccountId);
					throw new InvalidOperationException("В организации не настроено соответствие кабинета ЭДО");
				}
				
				_logger.LogInformation("Получаем заказы по которым надо создать и отправить УПД");
				var orders = _orderRepository.GetCashlessOrdersForEdoSend(uow, startDate, organization.Id);

				//Фильтруем заказы в которых есть УПД и которые не в пути, если у клиента стоит выборка по статусу доставлен
				var filteredOrders =
					orders.Where(o => o.Client.OrderStatusForSendingUpd != OrderStatusForSendingUpd.Delivered
										|| o.OrderStatus != OrderStatus.OnTheWay)
						.Where(o => o.OrderDocuments.Any(
							x => x.Type == OrderDocumentType.UPD || x.Type == OrderDocumentType.SpecialUPD)).ToList();

				_logger.LogInformation("Всего заказов для формирования УПД и отправки: {FilteredOrdersCount}", filteredOrders.Count);
				foreach(var order in filteredOrders)
				{
					_logger.LogInformation("Создаем УПД по заказу №{OrderId}", order.Id);
					try
					{
						var updXml = _edoUpdFactory.CreateNewUpdXml(order, edoAccountId, _certificate.Subject);
						var container = new TaxcomContainer
						{
							SignMode = DocumentSignMode.UseSpecifiedCertificate
						};

						var upd = new UniversalInvoiceDocument();
						UniversalInvoiceConverter.Convert(upd, updXml);

						if(!upd.Validate(out var errors))
						{
							var errorsString = string.Join(", ", errors);
							_logger.LogError("УПД {OrderId} не прошла валидацию\nОшибки: {ErrorsString}", order.Id, errorsString);
							continue;
						}

						container.Documents.Add(upd);
						upd.AddCertificateForSign(_certificate.Thumbprint);

						var containerRawData = container.ExportToZip();

						var edoContainer = new EdoContainer
						{
							Type = Type.Upd,
							Created = DateTime.Now,
							Container = containerRawData,
							Order = order,
							Counterparty = order.Client,
							MainDocumentId = $"{upd.FileIdentifier}.xml",
							EdoDocFlowStatus = EdoDocFlowStatus.NotStarted
						};

						var actions = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
							.Where(x => x.Order.Id == edoContainer.Order.Id)
							.FirstOrDefault();

						if(actions != null && actions.IsNeedToResendEdoUpd)
						{
							actions.IsNeedToResendEdoUpd = false;
						}

						_logger.LogInformation("Сохраняем контейнер по заказу №{OrderId}", order.Id);
						uow.Save(edoContainer);
						uow.Commit();

						_logger.LogInformation("Отправляем контейнер по заказу №{OrderId}", order.Id);
						_taxcomApi.Send(container);
					}
					catch(Exception e)
					{
						_logger.LogError(e, "Ошибка в процессе формирования УПД №{OrderId} и ее отправки", order.Id);
					}
				}
			}
			catch(Exception e)
			{
				isHealthy = false;
				_logger.LogError(e, "Ошибка в процессе получения заказов для формирования УПД");
			}

			return Task.FromResult(isHealthy);
		}

		private Task CreateAndSendBills(IUnitOfWork uow)
		{
			try
			{
				var startDate = DateTime.Today.AddDays(-3);
				var edoAccountId = _apiSection.GetValue<string>("EdxClientId");
				var organization = _organizationRepository.GetOrganizationByTaxcomEdoAccountId(uow, edoAccountId);

				if(organization is null)
				{
					_logger.LogError("Не найдена организация по edxClientId {EdoAccountId}", edoAccountId);
					throw new InvalidOperationException("В организации не настроено соответствие кабинета ЭДО");
				}

				_logger.LogInformation("Получаем заказы по которым нужно отправить счёт");
				var edoContainers = _edoContainersRepository
					.Get(uow, EdoContainerSpecification.CreateForAftedDateNotSendedWithOrganizationId(startDate, organization.Id))
					.ToList();

				//_orderRepository.GetPreparingToSendEdoContainers(uow, startDate, organization.Id);

				_logger.LogInformation("Всего заказов для формирования и отправки счёта: {OrdersCount}", edoContainers.Count);

				foreach(var edoContainer in edoContainers)
				{
					if(edoContainer.Order != null)
					{
						SendOrderContainer(uow, organization, edoContainer);
					}

					if(edoContainer.OrderWithoutShipmentForAdvancePayment != null)
					{
						SendOrderWithoutShipmentForAdvancePaymentContainer(uow, organization, edoContainer);
					}

					if(edoContainer.OrderWithoutShipmentForDebt != null)
					{
						SendOrderWithoutShipmentForDebtContainer(uow, organization, edoContainer);
					}

					if(edoContainer.OrderWithoutShipmentForPayment != null)
					{
						SendOrderWithoutShipmentForPaymentContainer(uow, organization, edoContainer);
					}
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе получения заказов для формирования счетов");
			}

			return Task.CompletedTask;
		}

		private void SendOrderContainer(IUnitOfWork unitOfWork, Organization organization, EdoContainer edoContainer)
		{
			_logger.LogInformation("Создаем счёт по заказу №{OrderId}", edoContainer.Order.Id);
			try
			{
				var container = new TaxcomContainer
				{
					SignMode = DocumentSignMode.UseSpecifiedCertificate
				};

				var orderDocumentTypes = new[] { OrderDocumentType.Bill, OrderDocumentType.SpecialBill };
				var printableRdlDocument = edoContainer.Order.OrderDocuments
					.FirstOrDefault(x => orderDocumentTypes.Contains(x.Type)) as IPrintableRDLDocument;
				var billAttachment = _printableDocumentSaver.SaveToPdf(printableRdlDocument);
				var fileName = $"Счёт №{edoContainer.Id} от {edoContainer.Order.CreateDate:d}.pdf";
				var document = _edoBillFactory.CreateBillDocument(edoContainer.Order, billAttachment, fileName, organization);

				container.Documents.Add(document);
				document.AddCertificateForSign(_certificate.Thumbprint);

				var containerRawData = container.ExportToZip();

				edoContainer.Container = containerRawData;
				edoContainer.MainDocumentId = document.ExternalIdentifier;
				edoContainer.EdoDocFlowStatus = EdoDocFlowStatus.NotStarted;

				_logger.LogInformation("Сохраняем контейнер №{EdoContainerId} по заказу №{OrderId}",
					edoContainer.Id,
					edoContainer.Order.Id);

				unitOfWork.Save(edoContainer);
				unitOfWork.Commit();

				_logger.LogInformation("Отправляем контейнер №{EdoContainerId} по заказу №{OrderId}",
					edoContainer.Id,
					edoContainer.Order.Id);

				_taxcomApi.Send(container);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе формирования контейнер №{EdoContainerId} счёта заказа №{OrderId} и его отправки",
					edoContainer.Id,
					edoContainer.Order.Id);
			}
		}

		private void SendOrderWithoutShipmentForPaymentContainer(IUnitOfWork unitOfWork, Organization organization, EdoContainer edoContainer)
		{
			_logger.LogInformation("Создаем счёт по заказу №{OrderId}", edoContainer.Order.Id);
			try
			{
				var container = new TaxcomContainer
				{
					SignMode = DocumentSignMode.UseSpecifiedCertificate
				};

				var orderDocumentTypes = new[] { OrderDocumentType.BillWSForPayment };
				var printableRdlDocument = edoContainer.OrderWithoutShipmentForPayment.Order.OrderDocuments
					.FirstOrDefault(x => orderDocumentTypes.Contains(x.Type)) as IPrintableRDLDocument;
				var billAttachment = _printableDocumentSaver.SaveToPdf(printableRdlDocument);
				var fileName = $"Счёт №{edoContainer.Id} от {edoContainer.OrderWithoutShipmentForPayment.CreateDate:d}.pdf";
				var document = _edoBillFactory.CreateBillWithoutShipmentForPaymentDocument(edoContainer.OrderWithoutShipmentForPayment, billAttachment, fileName, organization);

				container.Documents.Add(document);
				document.AddCertificateForSign(_certificate.Thumbprint);

				var containerRawData = container.ExportToZip();

				edoContainer.Container = containerRawData;
				edoContainer.MainDocumentId = document.ExternalIdentifier;
				edoContainer.EdoDocFlowStatus = EdoDocFlowStatus.NotStarted;

				_logger.LogInformation("Сохраняем контейнер №{EdoContainerId} по счету без отгрузки на постоплату №{OrderWithoutShipmentForPaymentId}",
					edoContainer.Id,
					edoContainer.OrderWithoutShipmentForPayment.Id);

				unitOfWork.Save(edoContainer);
				unitOfWork.Commit();

				_logger.LogInformation("Отправляем контейнер №{EdoContainerId} по счету без отгрузки на постоплату №{OrderWithoutShipmentForPaymentId}",
					edoContainer.Id,
					edoContainer.OrderWithoutShipmentForPayment.Id);

				_taxcomApi.Send(container);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе формирования контейнера №{EdoContainerId} счета без отгрузки на постоплату №{OrderWithoutShipmentForPaymentId} и его отправки",
					edoContainer.Id,
					edoContainer.OrderWithoutShipmentForPayment.Id);
			}
		}

		private void SendOrderWithoutShipmentForDebtContainer(IUnitOfWork unitOfWork, Organization organization, EdoContainer edoContainer)
		{
			_logger.LogInformation("Создаем счёт по заказу №{OrderId}", edoContainer.Order.Id);
			try
			{
				var container = new TaxcomContainer
				{
					SignMode = DocumentSignMode.UseSpecifiedCertificate
				};

				var orderDocumentTypes = new[] { OrderDocumentType.BillWSForDebt };
				var printableRdlDocument = edoContainer.OrderWithoutShipmentForDebt.Order.OrderDocuments
					.FirstOrDefault(x => orderDocumentTypes.Contains(x.Type)) as IPrintableRDLDocument;
				var billAttachment = _printableDocumentSaver.SaveToPdf(printableRdlDocument);
				var fileName = $"Счёт №{edoContainer.Id} от {edoContainer.OrderWithoutShipmentForDebt.CreateDate:d}.pdf";
				var document = _edoBillFactory.CreateBillWithoutShipmentForDebtDocument(edoContainer.OrderWithoutShipmentForDebt, billAttachment, fileName, organization);

				container.Documents.Add(document);
				document.AddCertificateForSign(_certificate.Thumbprint);

				var containerRawData = container.ExportToZip();

				edoContainer.Container = containerRawData;
				edoContainer.MainDocumentId = document.ExternalIdentifier;
				edoContainer.EdoDocFlowStatus = EdoDocFlowStatus.NotStarted;

				_logger.LogInformation("Сохраняем контейнер №{EdoContainerId} по счету без отгрузки на долг №{OrderWithoutShipmentForDebtId}",
					edoContainer.Id,
					edoContainer.OrderWithoutShipmentForDebt.Id);

				unitOfWork.Save(edoContainer);
				unitOfWork.Commit();

				_logger.LogInformation("Отправляем контейнер №{EdoContainerId} по счету без отгрузки на долг №{OrderWithoutShipmentForDebtId}",
					edoContainer.Id,
					edoContainer.OrderWithoutShipmentForDebt.Id);

				_taxcomApi.Send(container);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе формирования контейнера №{EdoContainerId} по счету без отгрузки на долг №{OrderWithoutShipmentForDebtId} и его отправки",
					edoContainer.Id,
					edoContainer.OrderWithoutShipmentForDebt.Id);
			}
		}

		private void SendOrderWithoutShipmentForAdvancePaymentContainer(IUnitOfWork unitOfWork, Organization organization, EdoContainer edoContainer)
		{
			_logger.LogInformation("Создаем счёт по заказу №{OrderId}", edoContainer.Order.Id);
			try
			{
				var container = new TaxcomContainer
				{
					SignMode = DocumentSignMode.UseSpecifiedCertificate
				};

				var orderDocumentTypes = new[] { OrderDocumentType.BillWSForAdvancePayment };
				var printableRdlDocument = edoContainer.OrderWithoutShipmentForAdvancePayment.Order.OrderDocuments
					.FirstOrDefault(x => orderDocumentTypes.Contains(x.Type)) as IPrintableRDLDocument;
				var billAttachment = _printableDocumentSaver.SaveToPdf(printableRdlDocument);
				var fileName = $"Счёт №{edoContainer.Id} от {edoContainer.OrderWithoutShipmentForAdvancePayment.CreateDate:d}.pdf";
				var document = _edoBillFactory.CreateBillWithoutShipmentForAdvancePaymentDocument(edoContainer.OrderWithoutShipmentForAdvancePayment, billAttachment, fileName, organization);

				container.Documents.Add(document);
				document.AddCertificateForSign(_certificate.Thumbprint);

				var containerRawData = container.ExportToZip();

				edoContainer.Container = containerRawData;
				edoContainer.MainDocumentId = document.ExternalIdentifier;
				edoContainer.EdoDocFlowStatus = EdoDocFlowStatus.NotStarted;

				_logger.LogInformation("Сохраняем контейнер №{EdoContainerId} по счету без отгрузки на предоплату №{OrderWithoutShipmentForAdvancePayment}",
					edoContainer.Id,
					edoContainer.OrderWithoutShipmentForAdvancePayment.Id);

				unitOfWork.Save(edoContainer);
				unitOfWork.Commit();

				_logger.LogInformation("Отправляем контейнер №{EdoContainerId} по счету без отгрузки на предоплату №{OrderWithoutShipmentForAdvancePayment}",
					edoContainer.Id,
					edoContainer.OrderWithoutShipmentForAdvancePayment.Id);

				_taxcomApi.Send(container);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе формирования контейнера №{EdoContainerId} по счету без отгрузки на предоплату №{OrderWithoutShipmentForAdvancePayment} и его отправки",
					edoContainer.Id,
					edoContainer.OrderWithoutShipmentForAdvancePayment.Id);
			}
		}

		private Task ProcessOutgoingDocuments(IUnitOfWork uow)
		{
			try
			{
				IDocFlowUpdates docFlowUpdates;
				do
				{
					_logger.LogInformation("Получаем исходящие документы");

					docFlowUpdates =
						_taxcomApi.GetDocflowsUpdates(null, _lastEventOutgoingDocumentsTimeStamp, DocFlowDirection.Outgoing, null, true);

					if(docFlowUpdates.Updates is null)
					{
						return Task.CompletedTask;
					}

					_logger.LogInformation("Обрабатываем полученные контейнеры {DocFlowUpdatesCount}", docFlowUpdates.Updates.Count);
					foreach(var item in docFlowUpdates.Updates)
					{
						EdoContainer container = null;

						if(item.Documents.Count > 0)
						{
							container = _orderRepository.GetEdoContainerByMainDocumentId(uow, item.Documents[0].ExternalIdentifier);
						}

						if(container != null)
						{
							var containerReceived =
								item.Documents.FirstOrDefault(x => x.TransactionCode == "PostDateConfirmation") != null;
							
							container.DocFlowId = item.Id;
							container.Received = containerReceived;
							container.InternalId = item.Documents[0].InternalId;
							container.ErrorDescription = item.ErrorDescription;
							container.EdoDocFlowStatus = Enum.Parse<EdoDocFlowStatus>(item.Status.ToString());
							
							if(container.EdoDocFlowStatus == EdoDocFlowStatus.Succeed)
							{
								container.Container = _taxcomApi.GetDocflowRawData(item.Id.Value.ToString());
							}
							
							_logger.LogInformation("Сохраняем изменения контейнера по заказу №{OrderId}", container.Order.Id);
							uow.Save(container);
							uow.Commit();
						}

						_lastEventOutgoingDocumentsTimeStamp = item.StatusChangeDateTime.ToBinary();
					}
				} while(!docFlowUpdates.IsLast);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе обработки исходящих документов");
			}
			finally
			{
				_parametersProvider.CreateOrUpdateParameter(
					"last_event_outgoing_documents_timestamp", _lastEventOutgoingDocumentsTimeStamp.ToString());
			}

			return Task.CompletedTask;
		}
		
		private Task ProcessIngoingDocuments(IUnitOfWork uow)
		{
			try
			{
				IDocFlowUpdates docFlowUpdates;
				do
				{
					_logger.LogInformation("Получаем входящие документы");
					docFlowUpdates =
						_taxcomApi.GetDocflowsUpdates(null, _lastEventIngoingDocumentsTimeStamp, DocFlowDirection.Ingoing, null, true);

					if(docFlowUpdates.Updates is null)
					{
						return Task.CompletedTask;
					}

					_logger.LogInformation("Сохраняем полученные документы");
					foreach(var item in docFlowUpdates.Updates)
					{
						var rawContainer = _taxcomApi.GetDocflowRawData(item.Id.Value.ToString());

						var client = _edoContainerMainDocumentIdParser.GetCounterpartyFromMainDocumentId(
							uow, item.Documents[0].ExternalIdentifier);

						var edoContainer = _orderRepository.GetEdoContainerByDocFlowId(uow, item.Id);

						if(edoContainer != null)
						{
							edoContainer.Container = rawContainer;
							edoContainer.EdoDocFlowStatus = Enum.Parse<EdoDocFlowStatus>(item.Status.ToString());
						}
						else
						{
							edoContainer = new EdoContainer
							{
								Container = rawContainer,
								IsIncoming = true,
								DocFlowId = item.Id,
								InternalId = item.Documents[0].InternalId,
								MainDocumentId = item.Documents[0].ExternalIdentifier,
								EdoDocFlowStatus = Enum.Parse<EdoDocFlowStatus>(item.Status.ToString()),
								Counterparty = client
							};
						}

						uow.Save(edoContainer);
						uow.Commit();
						_lastEventIngoingDocumentsTimeStamp = item.StatusChangeDateTime.ToBinary();
					}
				} while(!docFlowUpdates.IsLast);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе обработки входящих документов");
			}
			finally
			{
				_parametersProvider.CreateOrUpdateParameter(
					"last_event_ingoing_documents_timestamp", _lastEventIngoingDocumentsTimeStamp.ToString());
			}

			return Task.CompletedTask;
		}

		private async Task DelayAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Ждем {DelaySec}сек", _delaySec);
			await Task.Delay(_delaySec * 1000, stoppingToken);
		}
	}
}
