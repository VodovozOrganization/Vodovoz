using EdoDocumentsPreparer.Factories;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.OrdersWithoutShipment;
using TaxcomEdo.Library.Options;
using Vodovoz.Application.Documents;
using Vodovoz.Converters;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Settings.Delivery;
using Vodovoz.Zabbix.Sender;
using VodovozBusiness.Controllers;
using VodovozBusiness.Converters;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;

namespace EdoDocumentsPreparer
{
	public class EdoDocumentsPreparerWorker : BackgroundService
	{
		private static readonly IEnumerable<OrderDocumentType> _orderDocumentTypesForSendBill
			= new[] { OrderDocumentType.Bill, OrderDocumentType.SpecialBill };

		private readonly ILogger<EdoDocumentsPreparerWorker> _logger;
		private readonly IZabbixSender _zabbixSender;
		private readonly TaxcomEdoOptions _edoOptions;
		private readonly DocumentFlowOptions _documentFlowOptions;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private readonly IOrderConverter _orderConverter;
		private readonly IOrderWithoutShipmentConverter _orderWithoutShipmentConverter;
		private readonly IPaymentConverter _paymentConverter;
		private readonly PrintableDocumentSaver _printableDocumentSaver;
		private readonly IInfoForCreatingBillWithoutShipmentEdoFactory _billWithoutShipmentEdoInfoFactory;
		private readonly IInfoForCreatingEdoBillFactory _billInfoFactory;
		private readonly IFileDataFactory _fileDataFactory;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly IHostApplicationLifetime _applicationLifetime;
		private readonly ICounterpartyEdoAccountController _edoAccountController;
		private readonly DateTime _startDate;

		public EdoDocumentsPreparerWorker(
			ILogger<EdoDocumentsPreparerWorker> logger,
			IUserService userService,
			IZabbixSender zabbixSender,
			IOptions<TaxcomEdoOptions> edoOptions,
			IOptions<DocumentFlowOptions> documentFlowOptions,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrderRepository orderRepository,
			IOrganizationRepository organizationRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IOrderConverter orderConverter,
			IOrderWithoutShipmentConverter orderWithoutShipmentConverter,
			IPaymentConverter paymentConverter,
			PrintableDocumentSaver printableDocumentSaver,
			IInfoForCreatingBillWithoutShipmentEdoFactory billWithoutShipmentEdoInfoFactory,
			IInfoForCreatingEdoBillFactory billInfoFactory,
			IFileDataFactory fileDataFactory,
			IPublishEndpoint publishEndpoint,
			IHostApplicationLifetime applicationLifetime,
			ICounterpartyEdoAccountController edoAccountController)
		{
			_logger = logger;
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_documentFlowOptions = (documentFlowOptions ?? throw new ArgumentNullException(nameof(documentFlowOptions))).Value;
			_edoOptions = (edoOptions ?? throw new ArgumentNullException(nameof(edoOptions))).Value;
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			_orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
			_orderWithoutShipmentConverter =
				orderWithoutShipmentConverter ?? throw new ArgumentNullException(nameof(orderWithoutShipmentConverter));
			_paymentConverter = paymentConverter ?? throw new ArgumentNullException(nameof(paymentConverter));
			_printableDocumentSaver = printableDocumentSaver ?? throw new ArgumentNullException(nameof(printableDocumentSaver));
			_billWithoutShipmentEdoInfoFactory =
				billWithoutShipmentEdoInfoFactory ?? throw new ArgumentNullException(nameof(billWithoutShipmentEdoInfoFactory));
			_billInfoFactory = billInfoFactory ?? throw new ArgumentNullException(nameof(billInfoFactory));
			_fileDataFactory = fileDataFactory ?? throw new ArgumentNullException(nameof(fileDataFactory));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
			_applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
			_edoAccountController = edoAccountController ?? throw new ArgumentNullException(nameof(edoAccountController));
			_startDate = DateTime.Now;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

				try
				{
					if((DateTime.Now - _startDate).TotalDays > 0.2)
					{
						_logger.LogInformation("EdoDocumentsPreparerWorker termination to prevent memory leak at: {time}", DateTimeOffset.Now);
						_applicationLifetime.StopApplication();
						return;
					}

					await Task.Delay(1000 * _documentFlowOptions.DelayBetweenPreparingInSeconds, stoppingToken);

					using var uow = _unitOfWorkFactory.CreateWithoutRoot();

					var mainOrganization = (await GetOrganizations(uow, new[] { _edoOptions.OurMainEdoAccountId }, stoppingToken)).SingleOrDefault();

					if(mainOrganization != null)
					{
						await PrepareUpdDocumentsForSend(uow, mainOrganization.Id);						
					}
					else
					{
						var errorMessage = "Не настроена основная организация";

						_logger.LogError(errorMessage);

						await _zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem, errorMessage, stoppingToken);
					}

					var edoAccountIds = _edoOptions.OurEdoAccountsIds;
					var organizations = await GetOrganizations(uow, edoAccountIds, stoppingToken);

					if(organizations != null)
					{
						foreach(var organization in organizations)
						{
							await PrepareBillsForSend(uow, organization.Id, stoppingToken);

							await PrepareBillsWithoutShipmentForSend(uow, organization);
						}
					}
					else
					{
						var errorMessage = "Не настроены дополнительные организации";

						_logger.LogError(errorMessage);

						await _zabbixSender.SendProblemMessageAsync(ZabixSenderMessageType.Problem, errorMessage, stoppingToken);
					}

					await _zabbixSender.SendIsHealthyAsync(stoppingToken);
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка при запуске подготовки документов для отправки");
				}
			}
		}

		private async Task PrepareUpdDocumentsForSend(IUnitOfWork uow, int organizationId)
		{
			const string document = "УПД";
			_logger.LogInformation("Получаем заказы по которым надо создать и отправить {Document}", document);

			try
			{
				var startDate = DateTime.Today.AddMonths(_documentFlowOptions.AddMonthForUpdPreparing);
				var orders =
					_orderRepository.GetCashlessOrdersForEdoSendUpd(
						uow,
						startDate,
						organizationId,
						_deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId);

				var bulkAccountingEdoTasks =
					uow.GetAll<BulkAccountingEdoTask>()
						.Where(x => x.Status == EdoTaskStatus.New
							&& x.FormalEdoRequest.Order.PaymentType == PaymentType.Cashless)
						.ToList();

				//Фильтруем заказы в которых есть УПД и они не в пути, если у клиента стоит выборка по статусу доставлен
				var filteredOrdersDictionary = orders
					.Where(o => o.Client.OrderStatusForSendingUpd == OrderStatusForSendingUpd.EnRoute || o.OrderStatus != OrderStatus.OnTheWay)
					.Where(o => o.OrderDocuments.Any(
						x => x.Type == OrderDocumentType.UPD || x.Type == OrderDocumentType.SpecialUPD))
					.ToDictionary(x => x.Id);

				_logger.LogInformation(
					"Всего задач для формирования {Document} и отправки: {BulkAccountingEdoTasksCount}",
					document,
					bulkAccountingEdoTasks.Count);

				_logger.LogInformation(
					"Всего заказов для формирования {Document} и отправки: {FilteredOrdersCount}",
					document,
					filteredOrdersDictionary.Count);

				var i = 0;

				_logger.LogInformation("Обрабатываем новые отправки по таскам");
				while(i < bulkAccountingEdoTasks.Count)
				{
					var orderEntity = bulkAccountingEdoTasks[i].FormalEdoRequest.Order;

					if(!filteredOrdersDictionary.TryGetValue(orderEntity.Id, out var order))
					{
						_logger.LogWarning(
							"Пришла задача на формирование УПД по заказу {OrderId}, который не должен отправляться для объемного учета",
							orderEntity.Id);

						var container = uow
							.GetAll<EdoContainer>()
							.Where(x => x.Order.Id == orderEntity.Id && x.Type == DocumentContainerType.Upd)
							.OrderByDescending(x => x.Id)
							.FirstOrDefault();

						if(container != null)
						{
							_logger.LogWarning(
								"Контейнер с УПД по заказу {OrderId} уже отправлялся, обновляем информацию",
								orderEntity.Id);

							switch(container.EdoDocFlowStatus)
							{
								case EdoDocFlowStatus.Succeed:
									bulkAccountingEdoTasks[i].Status = EdoTaskStatus.Completed;
									break;
								case EdoDocFlowStatus.Cancelled:
								case EdoDocFlowStatus.NotAccepted:
								case EdoDocFlowStatus.Unknown:
								case EdoDocFlowStatus.WaitingForCancellation:
									break;
								case EdoDocFlowStatus.Error:
								case EdoDocFlowStatus.Warning:
								case EdoDocFlowStatus.CompletedWithDivergences:
									bulkAccountingEdoTasks[i].Status = EdoTaskStatus.Problem;
									//создать описание проблемы
									break;
								default:
									bulkAccountingEdoTasks[i].Status = EdoTaskStatus.InProgress;
									break;
							}

							container.EdoTaskId = bulkAccountingEdoTasks[i].Id;
							await uow.SaveAsync(container);
						}
						else
						{
							bulkAccountingEdoTasks[i].Status = EdoTaskStatus.Problem;
							//создать описание проблемы
						}

						await uow.SaveAsync(bulkAccountingEdoTasks[i]);
						await uow.CommitAsync();
						bulkAccountingEdoTasks.RemoveAt(i);
						continue;
					}

					var actionsForNew = uow
						.GetAll<OrderEdoTrueMarkDocumentsActions>()
						.Where(x => x.Order.Id == orderEntity.Id && x.IsNeedToResendEdoUpd)
						.ToArray();

					foreach(var action in actionsForNew)
					{
						action.IsNeedToResendEdoUpd = false;
						await uow.SaveAsync(action);
					}

					_logger.LogInformation("Отправляем {Document} по заказу {OrderId}",
						document,
						order.Id);
					await GenerateUpdAndSendMessage(uow, order, document, bulkAccountingEdoTasks[i]);
					bulkAccountingEdoTasks.RemoveAt(i);
					filteredOrdersDictionary.Remove(orderEntity.Id);
				}

				_logger.LogInformation("Обрабатываем оставшиеся заказы для формирования {Document} без тасок. Всего {FilteredOrdersCount}",
					document,
					filteredOrdersDictionary.Count);

				foreach(var keyPairValue in filteredOrdersDictionary)
				{
					var actionsForNew = uow
						.GetAll<OrderEdoTrueMarkDocumentsActions>()
						.Where(x => x.Order.Id == keyPairValue.Key && x.IsNeedToResendEdoUpd)
						.ToArray();

					foreach(var action in actionsForNew)
					{
						action.IsNeedToResendEdoUpd = false;
						await uow.SaveAsync(action);
					}

					_logger.LogInformation("Отправляем {Document} по заказу {OrderId}",
						document,
						keyPairValue.Key);
					await GenerateUpdAndSendMessage(uow, keyPairValue.Value, document);
				}

				_logger.LogInformation("Обрабатываем запросы на переотправку без тасок");

				var actionsWithoutTasks = uow
					.GetAll<OrderEdoTrueMarkDocumentsActions>()
					.Where(x => x.IsNeedToResendEdoUpd && x.Created > DateTime.Today)
					.ToLookup(x => x.Order.Id);

				foreach(var grouped in actionsWithoutTasks)
				{
					Order reSentOrder = null;
					foreach(var action in grouped)
					{
						reSentOrder = action.Order;
						action.IsNeedToResendEdoUpd = false;
						await uow.SaveAsync(action);
					}

					if(reSentOrder is null)
					{
						continue;
					}

					_logger.LogInformation("Отправляем {Document} по заказу {OrderId}",
						document,
						reSentOrder.Id);

					await GenerateUpdAndSendMessage(uow, reSentOrder, document);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе получения заказов для формирования {Document}", document);
			}
		}

		private async Task GenerateUpdAndSendMessage(
			IUnitOfWork uow,
			Order order,
			string document,
			BulkAccountingEdoTask task = null)
		{
			var orderPayments = _orderRepository.GetOrderPayments(uow, order.Id)
				.Where(p => order.DeliveryDate.HasValue && p.Date < order.DeliveryDate.Value.AddDays(1))
				.Distinct();

			var updInfo = InfoForCreatingEdoUpd.Create(
				_orderConverter.ConvertOrderToOrderInfoForEdo(order),
				_paymentConverter.ConvertPaymentToPaymentInfoForEdo(orderPayments));

			var edoContainer = EdoContainerBuilder
				.Create()
				.Empty()
				.OrderUpd(order)
				.MainDocumentId(updInfo.MainDocumentId.ToString())
				.Build();

			if(task != null)
			{
				edoContainer.EdoTaskId = task.Id;
			}

			_logger.LogInformation("Сохраняем контейнер с {Document} {OrderId}", document, order.Id);
			await uow.SaveAsync(edoContainer);
			await uow.CommitAsync();

			if(!await CheckCounterpartyConsentForEdo(uow, edoContainer, order.Id, order.Contract.Organization.Id, document, task))
			{
				return;
			}

			await SendUpdMessage(uow, document, order, updInfo, edoContainer, task);
		}

		private async Task SendUpdMessage(
			IUnitOfWork uow,
			string document,
			Order order,
			InfoForCreatingEdoUpd updInfo,
			EdoContainer edoContainer,
			BulkAccountingEdoTask task = null)
		{
			try
			{
				_logger.LogInformation("Отправляем данные по {Document} {OrderId} в очередь", document, order.Id);
				await _publishEndpoint.Publish(updInfo);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Не удалось отправить данные по {Document} {OrderId} в очередь",
					document,
					order.Id);

				await TrySaveContainerByErrorState(
					uow,
					edoContainer,
					order.Id,
					document,
					"Возникла ошибка при попытке отправки документа в очередь",
					task);
			}
		}

		private async Task PrepareBillsForSend(IUnitOfWork uow, int organizationId, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем заказы по которым нужно отправить счёт");

			try
			{
				var startDate = DateTime.Today.AddDays(_documentFlowOptions.AddDaysForBillsPreparing);
				var newOrdersToSend =
					_orderRepository.GetOrdersForEdoSendBills(
						uow, startDate, organizationId, _deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId);
				var ordersToResend = _orderRepository.GetOrdersForResendBills(uow, organizationId);

				var orders = newOrdersToSend.Union(ordersToResend).ToList();

				_logger.LogInformation("Всего заказов для формирования и отправки счёта: {OrdersCount}", orders.Count);

				await SendBillsData(uow, orders, cancellationToken);
			}
			catch(Exception e)
			{
				const string errorMessage = "Ошибка в процессе получения заказов для формирования счетов";
				_logger.LogError(e, errorMessage);
			}
		}

		private async Task SendBillsData(IUnitOfWork uow, IEnumerable<Order> orders, CancellationToken cancellationToken)
		{
			foreach(var order in orders)
			{
				try
				{
					if(order.OrderDocuments
						   .FirstOrDefault(x =>
							   _orderDocumentTypesForSendBill.Contains(x.Type)
							   && x.Order.Id == order.Id) is not IPrintableRDLDocument printableRdlDocument)
					{
						_logger.LogWarning("У заказа {OrderId} не найден документ для отправки счета", order.Id);
						continue;
					}

					var billAttachment = _printableDocumentSaver.SaveToPdf(printableRdlDocument);
					var orderInfo = _orderConverter.ConvertOrderToOrderInfoForEdo(order);
					var infoForCreatingEdoBill = _billInfoFactory.CreateInfoForCreatingEdoBill(
						orderInfo,
						_fileDataFactory.CreateBillFileData(orderInfo.Id.ToString(), orderInfo.CreationDate, billAttachment));

					var edoContainer = EdoContainerBuilder
						.Create()
						.Empty()
						.OrderBill(order)
						.MainDocumentId(infoForCreatingEdoBill.MainDocumentId.ToString())
						.Build();

					var actions = uow
						.GetAll<OrderEdoTrueMarkDocumentsActions>()
						.Where(x => x.Order.Id == edoContainer.Order.Id && x.IsNeedToResendEdoBill)
						.ToArray();

					foreach(var action in actions)
					{
						action.IsNeedToResendEdoBill = false;
						await uow.SaveAsync(action);
					}

					_logger.LogInformation("Сохраняем контейнер по заказу №{OrderId} для отправки счета", order.Id);
					await uow.SaveAsync(edoContainer);
					await uow.CommitAsync();

					if(!await CheckCounterpartyConsentForEdo(uow, edoContainer, order.Id, order.Contract.Organization.Id, "Счет"))
					{
						continue;
					}

					await SendBillData(order, infoForCreatingEdoBill, cancellationToken);
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Ошибка в процессе подготовки счёта заказа №{OrderId} и его отправки",
						order.Id);
				}
			}
		}

		private async Task SendBillData(Order order, InfoForCreatingEdoBill infoForCreatingEdoBill, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Отправляем данные по счету {OrderId} в очередь", order.Id);

				await _publishEndpoint.Publish(infoForCreatingEdoBill, cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Не удалось отправить данные по счету {OrderId} в очередь", order.Id);
			}
		}

		private async Task PrepareBillsWithoutShipmentForSend(IUnitOfWork uow, Organization organization)
		{
			_logger.LogInformation("Получаем заказы по которым нужно отправить счёта без отгрузки");

			try
			{
				var resendFromActions =
					uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
						.Where(x => x.IsNeedToResendEdoBill && x.Order == null)
						.ToList();

				_logger.LogInformation("Всего заказов для переотправки счётов без отгрузки: {OrdersCount}", resendFromActions.Count);

				foreach(var action in resendFromActions)
				{
					var now = DateTime.Now;

					var edoContainerBuilder = EdoContainerBuilder
						.Create()
						.Empty();

					var infoForCreatingBillWithoutShipmentEdo =
						GetBillWithoutShipmentDataForSend(action, edoContainerBuilder, organization, now);

					if(infoForCreatingBillWithoutShipmentEdo is null)
					{
						continue;
					}

					action.IsNeedToResendEdoBill = false;
					await uow.SaveAsync(action);

					var edoContainer = edoContainerBuilder.Build();

					_logger.LogInformation(
						"Сохраняем контейнер по {OrderWithoutShipment} {OrderId}",
						infoForCreatingBillWithoutShipmentEdo.GetBillWithoutShipmentInfoTitle(),
						infoForCreatingBillWithoutShipmentEdo.OrderWithoutShipmentInfo.Id);

					await uow.SaveAsync(edoContainer);
					await uow.CommitAsync();

					if(!await CheckCounterpartyConsentForEdo(
						uow,
						edoContainer,
						infoForCreatingBillWithoutShipmentEdo.OrderWithoutShipmentInfo.Id,
						organization.Id,
						infoForCreatingBillWithoutShipmentEdo.GetBillWithoutShipmentInfoTitle()))
					{
						continue;
					}

					try
					{
						_logger.LogInformation(
							"Отправляем данные по {OrderWithoutShipment} {OrderId} в очередь",
							infoForCreatingBillWithoutShipmentEdo.GetBillWithoutShipmentInfoTitle(),
							infoForCreatingBillWithoutShipmentEdo.OrderWithoutShipmentInfo.Id);

						switch(infoForCreatingBillWithoutShipmentEdo)
						{
							case InfoForCreatingBillWithoutShipmentForDebtEdo billForDebt:
								await _publishEndpoint.Publish(billForDebt);
								break;
							case InfoForCreatingBillWithoutShipmentForPaymentEdo billForPayment:
								await _publishEndpoint.Publish(billForPayment);
								break;
							case InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo billForAdvancePayment:
								await _publishEndpoint.Publish(billForAdvancePayment);
								break;
						}
					}
					catch(Exception e)
					{
						_logger.LogError(
							e,
							"Не удалось отправить данные по {OrderWithoutShipment} {OrderId} в очередь",
							infoForCreatingBillWithoutShipmentEdo.GetBillWithoutShipmentInfoTitle(),
							infoForCreatingBillWithoutShipmentEdo.OrderWithoutShipmentInfo.Id);
					}
				}
			}
			catch(Exception e)
			{
				const string errorMessage = "Ошибка в процессе получения счетов без отгрузки";
				_logger.LogError(e, errorMessage);
			}
		}

		private InfoForCreatingBillWithoutShipmentEdo GetBillWithoutShipmentDataForSend(
			OrderEdoTrueMarkDocumentsActions orderEdoActions,
			EdoContainerBuilder edoContainerBuilder,
			Organization organization,
			DateTime now)
		{
			OrderWithoutShipmentInfo orderWithoutShipmentInfo = null;
			byte[] billAttachment = null;

			if(orderEdoActions.OrderWithoutShipmentForDebt is OrderWithoutShipmentForDebt orderWithoutShipmentForDebt)
			{
				orderWithoutShipmentInfo =
					_orderWithoutShipmentConverter.ConvertOrderWithoutShipmentForDebtToOrderWithoutShipmentInfo(
						orderWithoutShipmentForDebt, organization, now);
				billAttachment = _printableDocumentSaver.SaveToPdf(orderWithoutShipmentForDebt);

				edoContainerBuilder.BillWithoutShipmentForDebt(orderWithoutShipmentForDebt);
			}

			if(orderEdoActions.OrderWithoutShipmentForPayment is OrderWithoutShipmentForPayment orderWithoutShipmentForPayment)
			{
				orderWithoutShipmentInfo =
					_orderWithoutShipmentConverter.ConvertOrderWithoutShipmentForPaymentToOrderWithoutShipmentInfo(
						orderWithoutShipmentForPayment, organization, now);
				billAttachment = _printableDocumentSaver.SaveToPdf(orderWithoutShipmentForPayment);

				edoContainerBuilder.BillWithoutShipmentForPayment(orderWithoutShipmentForPayment);
			}

			if(orderEdoActions.OrderWithoutShipmentForAdvancePayment is OrderWithoutShipmentForAdvancePayment orderWithoutShipmentForAdvancePayment)
			{
				orderWithoutShipmentInfo =
					_orderWithoutShipmentConverter.ConvertOrderWithoutShipmentForAdvancePaymentToOrderWithoutShipmentInfo(
						orderWithoutShipmentForAdvancePayment, organization, now);
				billAttachment = _printableDocumentSaver.SaveToPdf(orderWithoutShipmentForAdvancePayment);

				edoContainerBuilder.BillWithoutShipmentForAdvancePayment(orderWithoutShipmentForAdvancePayment);
			}

			if(orderWithoutShipmentInfo is null)
			{
				_logger.LogWarning("Не подобрать счет без отгрузки для отправки по ЭДО");
				return null;
			}

			var infoForCreatingBillWithoutShipmentEdo = _billWithoutShipmentEdoInfoFactory.CreateInfoForCreatingBillWithoutShipmentEdo(
				orderWithoutShipmentInfo,
				_fileDataFactory.CreateBillFileData(
					orderWithoutShipmentInfo.BillNumber, orderWithoutShipmentInfo.CreationDate, billAttachment));

			edoContainerBuilder.MainDocumentId(infoForCreatingBillWithoutShipmentEdo.MainDocumentId.ToString());

			return infoForCreatingBillWithoutShipmentEdo;
		}

		private async Task<bool> CheckCounterpartyConsentForEdo(
			IUnitOfWork uow,
			EdoContainer edoContainer,
			int documentId,
			int organizationId,
			string document,
			BulkAccountingEdoTask task = null)
		{
			var edoAccount =
				_edoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(edoContainer.Counterparty, organizationId);

			if(edoAccount is null
				|| string.IsNullOrWhiteSpace(edoAccount.PersonalAccountIdInEdo)
				|| edoAccount.ConsentForEdoStatus != ConsentForEdoStatus.Agree)
			{
				await TrySaveContainerByErrorState(
					uow, edoContainer, documentId, document, "У клиента не заполнен номер кабинета или нет согласия на ЭДО", task);
				return false;
			}

			return true;
		}

		private async Task TrySaveContainerByErrorState(
			IUnitOfWork uow,
			EdoContainer edoContainer,
			int documentId,
			string document,
			string errorMessage,
			BulkAccountingEdoTask task = null)
		{
			try
			{
				edoContainer.EdoDocFlowStatus = EdoDocFlowStatus.Error;
				edoContainer.ErrorDescription = errorMessage;

				if(task != null)
				{
					//сделать описание проблемы
					task.Status = EdoTaskStatus.Problem;
					await uow.SaveAsync(task);
				}

				await uow.SaveAsync(edoContainer);
				await uow.CommitAsync();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Не удалось сохранить контейнер по {Document} {DocumentId} в состоянии ошибки {ErrorMessage}",
					document,
					documentId,
					errorMessage);
			}
		}

		private async Task<IList<Organization>> GetOrganizations(IUnitOfWork uow, string[] edoAccountIds, CancellationToken cancellationToken)
		{
			var organizations = await _organizationRepository.GetOrganizationsByTaxcomEdoAccountIds(uow, edoAccountIds, cancellationToken);

			if(organizations is null)
			{
				_logger.LogError("Не найдены организации по edxClientId {EdoAccountId}", edoAccountIds);
				return null;
			}

			var organizationsNames = string.Join(",", organizations.Select(x => $"{x.Id}:{x.FullName}"));

			_logger.LogInformation(
				"Найдены организация по edxClientId {EdoAccountId} - {OrganizationName}",
				edoAccountIds, organizationsNames);

			return organizations;
		}
	}
}
