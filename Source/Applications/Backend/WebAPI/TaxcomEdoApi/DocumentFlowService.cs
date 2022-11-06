using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Taxcom.Client.Api;
using Taxcom.Client.Api.Converters;
using Taxcom.Client.Api.Entity;
using Taxcom.Client.Api.Entity.DocFlow;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Parameters;
using Vodovoz.Tools.Orders;

namespace TaxcomEdoApi
{
	public class DocumentFlowService : BackgroundService
	{
		private readonly ILogger<DocumentFlowService> _logger;
		private readonly TaxcomApi _taxcomApi;
		private readonly IParametersProvider _parametersProvider;
		private readonly IOrderRepository _orderRepository;
		private readonly EdoUpdFactory _edoUpdFactory;
		private readonly EdoContainerMainDocumentIdParser _edoContainerMainDocumentIdParser;
		private const int _delaySec = 90;

		private long? _lastEventIngoingDocumentsTimeStamp;
		private long? _lastEventOutgoingDocumentsTimeStamp;

		public DocumentFlowService(
			ILogger<DocumentFlowService> logger,
			TaxcomApi taxcomApi,
			IParametersProvider parametersProvider,
			IOrderRepository orderRepository,
			EdoUpdFactory edoUpdFactory,
			EdoContainerMainDocumentIdParser edoContainerMainDocumentIdParser)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApi = taxcomApi ?? throw new ArgumentNullException(nameof(taxcomApi));
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_edoUpdFactory = edoUpdFactory ?? throw new ArgumentNullException(nameof(edoUpdFactory));
			_edoContainerMainDocumentIdParser =
				edoContainerMainDocumentIdParser ?? throw new ArgumentNullException(nameof(edoContainerMainDocumentIdParser));
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
			var certificates = CertificateLogic.GetAvailableCertificates();

			while(!stoppingToken.IsCancellationRequested)
			{
				await DelayAsync(stoppingToken);
				
				var startDate = new DateTime(2015, 1, 1)/*DateTime.Today.AddDays(-3)*/;

				using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					await CreateAndSendUpd(uow, startDate, certificates);
					await ProcessOutgoingDocuments(uow);
					await ProcessIngoingDocuments(uow);
				}
			}
		}
		
		private Task CreateAndSendUpd(IUnitOfWork uow, DateTime startDate, IList<X509Certificate2> certificates)
		{
			try
			{
				_logger.LogInformation("Получаем заказы по которым надо создать и отправить УПД");
				var orders = _orderRepository.GetCashlessOrdersForEdoSend(uow, startDate);

				//Фильтруем заказы в которых есть УПД и которые не в пути, если у клиента стоит выборка по статусу доставлен
				var filteredOrders =
					orders.Where(o => o.Client.OrderStatusForSendingUpd != OrderStatusForSendingUpd.Delivered
										|| o.OrderStatus != OrderStatus.OnTheWay)
						.Where(o => o.OrderDocuments.Any(
							x => x.Type == OrderDocumentType.UPD || x.Type == OrderDocumentType.SpecialUPD)).ToList();

				_logger.LogInformation($"Всего заказов для формирования УПД и отправки: {filteredOrders.Count}");
				foreach(var order in filteredOrders)
				{
					_logger.LogInformation($"Создаем УПД по заказу №{order.Id}");
					try
					{
						var updXml = _edoUpdFactory.CreateNewUpdXml(order);
						var container = new TaxcomContainer
						{
							SignMode = DocumentSignMode.UseSpecifiedCertificate
						};

						var upd = new UniversalInvoiceDocument();
						UniversalInvoiceConverter.Convert(upd, updXml);

						upd.Validate(out var errors);

						container.Documents.Add(upd);
						upd.AddCertificateForSign(certificates[1].Thumbprint);

						var containerRawData = container.ExportToZip();

						var edoContainer = new EdoContainer
						{
							Container = containerRawData,
							Order = order,
							MainDocumentId = $"{upd.FileIdentifier}.xml",
							EdoContainerStatus = EdoContainerStatus.NotStarted
						};

						_logger.LogInformation($"Сохраняем контейнер по заказу №{order.Id}");
						uow.Save(edoContainer);
						uow.Commit();

						_logger.LogInformation($"Отправляем контейнер по заказу №{order.Id}");
						_taxcomApi.Send(container);
					}
					catch(Exception e)
					{
						_logger.LogError(e, "Ошибка в процессе формирования УПД и ее отправки");
					}
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе получения заказов для формирования УПД");
			}

			return Task.CompletedTask;
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

					_logger.LogInformation($"Обрабатываем полученные контейнеры {docFlowUpdates.Updates.Count}");
					foreach(var item in docFlowUpdates.Updates)
					{
						EdoContainer container = null;

						switch(item.Status)
						{
							case DocFlowUpdateStatus.Error:
							case DocFlowUpdateStatus.Unknown:
							case DocFlowUpdateStatus.Warning:
							case DocFlowUpdateStatus.NotAccepted:
							case DocFlowUpdateStatus.NotStarted:
							case DocFlowUpdateStatus.CompletedWithDivergences:
							case DocFlowUpdateStatus.InProgress:
								container = _orderRepository.GetEdoContainerByMainDocumentId(uow, item.Documents[0].ExternalIdentifier);
								var containerReceived = false;

								if(item.Status == DocFlowUpdateStatus.InProgress)
								{
									containerReceived =
										item.Documents.SingleOrDefault(x => x.TransactionCode == "PostDateConfirmation") != null;
								}

								if(container != null)
								{
									container.DocFlowId = item.Id;
									container.InternalId = item.Documents[0].InternalId;
									container.ErrorDescription = item.ErrorDescription;
									container.EdoContainerStatus = Enum.Parse<EdoContainerStatus>(item.Status.ToString());
									container.Received = containerReceived;
								}

								break;
							case DocFlowUpdateStatus.Succeed:
								container = _orderRepository.GetEdoContainerByMainDocumentId(uow, item.Documents[0].ExternalIdentifier);

								var rawContainer = _taxcomApi.GetDocflowRawData(item.Id.Value.ToString());

								if(container != null)
								{
									container.Container = rawContainer;
								}

								break;
						}

						if(container != null)
						{
							_logger.LogInformation($"Сохраняем изменения контейнера по заказу №{container.Order.Id}");
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

						var container = new EdoContainer
						{
							Container = rawContainer,
							IsIncoming = true,
							DocFlowId = item.Id,
							InternalId = item.Documents[0].InternalId,
							MainDocumentId = item.Documents[0].ExternalIdentifier,
							EdoContainerStatus = Enum.Parse<EdoContainerStatus>(item.Status.ToString()),
							Counterparty = client
						};

						uow.Save(container);
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
			_logger.LogInformation($"Ждем {_delaySec}сек");
			await Task.Delay(_delaySec * 1000, stoppingToken);
		}
	}
}
