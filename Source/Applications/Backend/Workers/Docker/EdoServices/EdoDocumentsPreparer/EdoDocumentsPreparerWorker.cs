using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using QS.Report;
using TaxcomEdo.Contracts;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.OrdersWithoutShipment;
using TaxcomEdo.Library.Options;
using Vodovoz.Converters;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Settings;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Converters;

namespace EdoDocumentsPreparer
{
	public class EdoDocumentsPreparerWorker : BackgroundService
	{
		private static IEnumerable<OrderDocumentType> _orderDocumentTypesForSendBill
			= new[] { OrderDocumentType.Bill, OrderDocumentType.SpecialBill };
		
		private readonly ILogger<EdoDocumentsPreparerWorker> _logger;
		private readonly DocumentFlowOptions _documentFlowOptions;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISettingsController _settingController;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private readonly IOrderConverter _orderConverter;
		private readonly IOrderWithoutShipmentConverter _orderWithoutShipmentConverter;
		private readonly IPaymentConverter _paymentConverter;
		private readonly PrintableDocumentSaver _printableDocumentSaver;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly TaxcomEdoOptions _edoOptions;
		
		private int _closingDocumentDeliveryScheduleId;
		
		public EdoDocumentsPreparerWorker(
			ILogger<EdoDocumentsPreparerWorker> logger,
			IOptions<TaxcomEdoOptions> edoOptions,
			IOptions<DocumentFlowOptions> documentFlowOptions,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISettingsController settingController,
			IOrderRepository orderRepository,
			IOrganizationRepository organizationRepository,
			IOrganizationSettings organizationSettings,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IOrderConverter orderConverter,
			IOrderWithoutShipmentConverter orderWithoutShipmentConverter,
			IPaymentConverter paymentConverter,
			PrintableDocumentSaver printableDocumentSaver,
			IPublishEndpoint publishEndpoint)
		{
			_logger = logger;
			_documentFlowOptions = (documentFlowOptions ?? throw new ArgumentNullException(nameof(documentFlowOptions))).Value;
			_edoOptions = (edoOptions ?? throw new ArgumentNullException(nameof(edoOptions))).Value;
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_settingController = settingController ?? throw new ArgumentNullException(nameof(settingController));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			_orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
			_orderWithoutShipmentConverter =
				orderWithoutShipmentConverter ?? throw new ArgumentNullException(nameof(orderWithoutShipmentConverter));
			_paymentConverter = paymentConverter ?? throw new ArgumentNullException(nameof(paymentConverter));
			_printableDocumentSaver = printableDocumentSaver ?? throw new ArgumentNullException(nameof(printableDocumentSaver));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				await Task.Delay(1000 * _documentFlowOptions.DelayBetweenPreparingSeconds, stoppingToken);
				
				using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
				{
					var organization = GetOrganization(uow);

					if(organization is null)
					{
						continue;
					}
					
					await PrepareUpdDocumentsForSend(uow, organization.Id);
					await PrepareBillsForSend(uow, organization.Id);
					await PrepareBillsWithoutShipmentForSend(uow, organization);
				}
			}
		}

		private async Task PrepareUpdDocumentsForSend(IUnitOfWork uow, int organizationId)
		{
			_logger.LogInformation("Получаем заказы по которым надо создать и отправить УПД");

			try
			{
				var startDate = DateTime.Today.AddMonths(_documentFlowOptions.AddMonthForUpdPreparing);
				var orders =
					_orderRepository.GetCashlessOrdersForEdoSendUpd(
						uow,
						startDate,
						organizationId,
						_deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId);

				//Фильтруем заказы в которых есть УПД и которые не в пути, если у клиента стоит выборка по статусу доставлен
				var filteredOrders =
					orders.Where(o => o.Client.OrderStatusForSendingUpd != OrderStatusForSendingUpd.Delivered
									|| o.OrderStatus != OrderStatus.OnTheWay)
						.Where(o => o.OrderDocuments.Any(
							x => x.Type == OrderDocumentType.UPD || x.Type == OrderDocumentType.SpecialUPD)).ToList();
			
				_logger.LogInformation("Всего заказов для формирования УПД и отправки: {FilteredOrdersCount}", filteredOrders.Count);

				foreach(var order in filteredOrders)
				{
					try
					{
						var orderPayments = _orderRepository.GetOrderPayments(uow, order.Id)
							.Where(p => order.DeliveryDate.HasValue && p.Date < order.DeliveryDate.Value.AddDays(1))
							.Distinct();
						
						var updInfo = InfoForCreatingEdoUpd.Create(
							_orderConverter.ConvertOrderToOrderInfoForEdo(order),
							_paymentConverter.ConvertPaymentToPaymentInfoForEdo(orderPayments));

						try
						{
							_logger.LogInformation("Отправляем данные по УПД {OrderId} в очередь", order.Id);
							await _publishEndpoint.Publish(updInfo);
						}
						catch(Exception e)
						{
							_logger.LogError(e, "Не удалось отправить данные по УПД {OrderId} в очередь", order.Id);
							continue;
						}

						var edoContainer = EdoContainerBuilder.Create()
							.Empty()
							.EmptyContainer()
							.OrderUpd(order)
							.MainDocumentId(updInfo.MainDocumentId.ToString())
							.Build();
						
						/*new EdoContainer
						{
							Type = Type.Upd,
							Created = DateTime.Now,
							Container = new byte[64],
							Order = order,
							Counterparty = order.Client,
							MainDocumentId = updInfo.MainDocumentId.ToString(),
							EdoDocFlowStatus = EdoDocFlowStatus.NotStarted
						};*/

						var actions = uow
							.GetAll<OrderEdoTrueMarkDocumentsActions>()
							.FirstOrDefault(x => x.Order.Id == edoContainer.Order.Id);

						if(actions != null && actions.IsNeedToResendEdoUpd)
						{
							actions.IsNeedToResendEdoUpd = false;
							uow.Save(actions);
						}

						_logger.LogInformation("Сохраняем контейнер с УПД {OrderId}", order.Id);
						uow.Save(edoContainer);
						uow.Commit();
					}
					catch(Exception e)
					{
						_logger.LogError(e, "Ошибка в процессе формирования УПД №{OrderId} и ее отправки", order.Id);
					}
				}
			}
			catch(Exception e)
			{
				const string message = "Ошибка в процессе получения заказов для формирования УПД";
				//_taxcomEdoApiHealthCheck.HealthResult.IsHealthy = false;
				//_taxcomEdoApiHealthCheck.HealthResult.AdditionalUnhealthyResults.Add($"{message}: {e.Message}");
				_logger.LogError(e, message);
			}
		}

		private async Task PrepareBillsForSend(IUnitOfWork uow, int organizationId)
		{
			_logger.LogInformation("Получаем заказы по которым нужно отправить счёт");

			try
			{
				var startDate = DateTime.Today.AddDays(_documentFlowOptions.AddDaysForBillsPreparing);
				var orders =
					_orderRepository.GetOrdersForEdoSendBills(
						uow, startDate, organizationId, _deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId);

				_logger.LogInformation("Всего заказов для формирования и отправки счёта: {OrdersCount}", orders.Count);

				foreach(var order in orders)
				{
					try
					{
						var printableRdlDocument = order.OrderDocuments
							.FirstOrDefault(x =>
								_orderDocumentTypesForSendBill.Contains(x.Type)
									&& x.Order.Id == order.Id) as IPrintableRDLDocument;
						var billAttachment = _printableDocumentSaver.SaveToPdf(printableRdlDocument);
						var orderInfo = _orderConverter.ConvertOrderToOrderInfoForEdo(order);
						var infoForCreatingEdoBill = InfoForCreatingEdoBill.Create(
							orderInfo,
							BillFileData.Create(orderInfo.Id.ToString(), orderInfo.CreationDate, billAttachment));

						try
						{
							_logger.LogInformation("Отправляем данные по счету {OrderId} в очередь", order.Id);
							await _publishEndpoint.Publish(infoForCreatingEdoBill);
						}
						catch(Exception e)
						{
							_logger.LogError(e, "Не удалось отправить данные по счету {OrderId} в очередь", order.Id);
							continue;
						}

						var edoContainer = EdoContainerBuilder.Create()
							.Empty()
							.EmptyContainer()
							.OrderBill(order)
							.MainDocumentId(infoForCreatingEdoBill.MainDocumentId.ToString())
							.Build();
							
						/*new EdoContainer
						{
							Type = Type.Bill,
							Created = DateTime.Now,
							Container = new byte[64],
							Order = order,
							Counterparty = order.Client,
							MainDocumentId = infoForCreatingEdoBill.MainDocumentId.ToString(),
							EdoDocFlowStatus = EdoDocFlowStatus.NotStarted
						};*/

						var action = uow
							.GetAll<OrderEdoTrueMarkDocumentsActions>()
							.FirstOrDefault(x => x.Order.Id == edoContainer.Order.Id);

						if(action != null && action.IsNeedToResendEdoBill)
						{
							action.IsNeedToResendEdoUpd = false;
							uow.Save(action);
						}

						_logger.LogInformation("Сохраняем контейнер по заказу №{OrderId}", order.Id);
						uow.Save(edoContainer);
						uow.Commit();
					}
					catch(Exception e)
					{
						_logger.LogError(e, "Ошибка в процессе подготовки счёта заказа №{OrderId} и его отправки",
							order.Id);
					}
				}
			}
			catch(Exception e)
			{
				const string message = "Ошибка в процессе получения заказов для формирования счетов";
				//_taxcomEdoApiHealthCheck.HealthResult.IsHealthy = false;
				//_taxcomEdoApiHealthCheck.HealthResult.AdditionalUnhealthyResults.Add($"{message}: {e.Message}");
				_logger.LogError(e, message);
			}
		}

		private async Task PrepareBillsWithoutShipmentForSend(IUnitOfWork uow, Organization organization)
		{
			_logger.LogInformation("Получаем заказы по которым нужно отправить счёта без отгрузки");

			try
			{
				var resendFromActions =
					uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
						.Where(x => x.IsNeedToResendEdoBill && x.Order.Id == null)
						.ToList();

				_logger.LogInformation("Всего заказов для переотправки счётов без отгрузки: {OrdersCount}", resendFromActions.Count);

				foreach(var action in resendFromActions)
				{
					var now = DateTime.Now;

					/*var edoContainer = new EdoContainer
					{
						Created = now,
						Container = new byte[64],
						EdoDocFlowStatus = EdoDocFlowStatus.NotStarted
					};*/

					var edoContainerBuilder = EdoContainerBuilder.Create()
						.Empty()
						.EmptyContainer();

					var infoForCreatingBillWithoutShipmentEdo =
						GetBillWithoutShipmentDataForSend(action, edoContainerBuilder, organization, now);

					if(infoForCreatingBillWithoutShipmentEdo is null)
					{
						continue;
					}

					try
					{
						_logger.LogInformation(
							"Отправляем данные по счету без отгрузки {OrderId} в очередь",
							infoForCreatingBillWithoutShipmentEdo.OrderWithoutShipmentInfo.Id);
						await _publishEndpoint.Publish(infoForCreatingBillWithoutShipmentEdo);
					}
					catch(Exception e)
					{
						_logger.LogError(
							e,
							"Не удалось отправить данные по счету без отгрузки {OrderId} в очередь",
							infoForCreatingBillWithoutShipmentEdo.OrderWithoutShipmentInfo.Id);
						continue;
					}

					action.IsNeedToResendEdoBill = false;
					uow.Save(action);

					_logger.LogInformation(
						"Сохраняем контейнер по счету без отгрузки {OrderId}",
						infoForCreatingBillWithoutShipmentEdo.OrderWithoutShipmentInfo.Id);
					uow.Save(edoContainerBuilder.Build());
					uow.Commit();
				}
			}
			catch(Exception e)
			{
				const string message = "Ошибка в процессе получения счетов без отгрузки";
				//_taxcomEdoApiHealthCheck.HealthResult.IsHealthy = false;
				//_taxcomEdoApiHealthCheck.HealthResult.AdditionalUnhealthyResults.Add($"{message}: {e.Message}");
				_logger.LogError(e, message);
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
			
			var infoForCreatingBillWithoutShipmentEdo = InfoForCreatingBillWithoutShipmentEdo.Create(
				orderWithoutShipmentInfo,
				BillFileData.Create(orderWithoutShipmentInfo.BillNumber, orderWithoutShipmentInfo.CreationDate, billAttachment));
			
			edoContainerBuilder.MainDocumentId(infoForCreatingBillWithoutShipmentEdo.MainDocumentId.ToString());
			
			return infoForCreatingBillWithoutShipmentEdo;
		}

		private Organization GetOrganization(IUnitOfWork uow)
		{
			var edoAccountId = _edoOptions.EdxClientId;
			var organization = _organizationRepository.GetOrganizationByTaxcomEdoAccountId(uow, edoAccountId);

			if(organization is null)
			{
				_logger.LogError("Не найдена организация по edxClientId {EdoAccountId}", edoAccountId);
				return null;
			}

			_logger.LogInformation(
				"Найдена организация по edxClientId {EdoAccountId} - [{OrganizationId}]:\"{OrganizationName}\"",
				edoAccountId, organization.Id, organization.FullName);
			
			return organization;
		}
	}
}
