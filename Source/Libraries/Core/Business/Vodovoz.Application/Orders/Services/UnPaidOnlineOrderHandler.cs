using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Extensions;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class UnPaidOnlineOrderHandler : IUnPaidOnlineOrderHandler
	{
		private readonly ILogger<UnPaidOnlineOrderHandler> _logger;
		private readonly IOrderSettings _orderSettings;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderOnlinePaymentAcceptanceHandler _onlinePaymentAcceptanceHandler;
		private readonly IOrderFromOnlineOrderValidator _onlineOrderValidator;
		private readonly IOrderService _orderService;
		private readonly IRouteListService _routeListService;

		public UnPaidOnlineOrderHandler(
			ILogger<UnPaidOnlineOrderHandler> logger,
			IOrderSettings orderSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOrderRepository orderRepository,
			IOrderOnlinePaymentAcceptanceHandler onlinePaymentAcceptanceHandler,
			IOrderFromOnlineOrderValidator onlineOrderValidator,
			IOrderService orderService,
			IRouteListService routeListService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_onlinePaymentAcceptanceHandler =
				onlinePaymentAcceptanceHandler ?? throw new ArgumentNullException(nameof(onlinePaymentAcceptanceHandler));
			_onlineOrderValidator = onlineOrderValidator ?? throw new ArgumentNullException(nameof(onlineOrderValidator));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
		}

		public async Task TryMoveToManualProcessingWaitingForPaymentOnlineOrders(IUnitOfWork uow)
		{
			_logger.LogInformation("Проверяем онлайн заказы, ожидающих оплаты...");

			try
			{
				var waitingForPaymentOnlineOrders = _onlineOrderRepository.GetWaitingForPaymentOnlineOrders(uow);
				_logger.LogInformation("Найдено {WaitingForPaymentCount} онлайн заказов", waitingForPaymentOnlineOrders.Count());

				if(!waitingForPaymentOnlineOrders.Any())
				{
					return;
				}

				var onlineOrderTimers = uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

				if(onlineOrderTimers is null)
				{
					_logger.LogWarning("Не найдены таймеры онлайн заказов");
					return;
				}

				foreach(var onlineOrder in waitingForPaymentOnlineOrders)
				{
					TransferToManualProcessing(onlineOrder,
						onlineOrder.IsFastDelivery
							? onlineOrderTimers.TimeForTransferToManualProcessingWithFastDelivery
							: onlineOrderTimers.TimeForTransferToManualProcessingWithoutFastDelivery);

					await uow.SaveAsync(onlineOrder);
				}

				_logger.LogInformation("Сохраняем обработанные заказы");
				await uow.CommitAsync();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке онлайн заказов, ожидающих оплаты");
			}
		}

		public Result CanChangePaymentType(IUnitOfWork uow, OnlineOrder onlineOrder)
		{
			if(onlineOrder is null)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.OnlineOrderNotFound);
			}

			if(onlineOrder.OnlineOrderStatus != OnlineOrderStatus.WaitingForPayment)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.IsOnlineOrderNotWaitForPayment);
			}

			var onlineOrderTimers = uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

			if(onlineOrderTimers is null)
			{
				_logger.LogWarning("Не найдены таймеры онлайн заказов");
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.IsOnlineOrderTimersEmpty);
			}

			var timeForPay = onlineOrder.IsFastDelivery
				? onlineOrderTimers.PayTimeWithFastDelivery
				: onlineOrderTimers.PayTimeWithoutFastDelivery;

			if((DateTime.Now - onlineOrder.Created).TotalSeconds >= timeForPay.TotalSeconds)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.HasTimeToPayOrderExpired);
			}
			
			if(onlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.Paid)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.IsOnlineOrderPaid);
			}

			if(onlineOrder.OnlineOrderPaymentType != OnlineOrderPaymentType.PaidOnline)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.CantChangePaymentType);
			}
			
			return Result.Success();
		}

		public async Task<Result> TryUpdateOrderAsync(
			IUnitOfWork uow,
			IEnumerable<Order> orders,
			OnlineOrder onlineOrder,
			DeliverySchedule deliverySchedule,
			UpdateOnlineOrderFromChangeRequest data,
			CancellationToken cancellationToken)
		{
			if(onlineOrder is null)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.OnlineOrderNotFound);
			}
			
			if(data.PaymentStatus == OnlineOrderPaymentStatus.Paid && !data.OnlinePayment.HasValue)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.OnlineOrderIsPaidButOnlinePaymentIsEmpty);
			}
			
			if(data.DeliveryScheduleId.HasValue && deliverySchedule is null)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.IsUnknownDeliverySchedule);
			}
			
			//у оплаченных мы меняем только информацию по доставке в нужных состояниях
			if(onlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.Paid)
			{
				return TryUpdatePaidOnlineOrder(uow, orders, onlineOrder, deliverySchedule, data);
			}
			else
			{
				if(orders.Any())
				{
					return TryUpdateUnPaidOnlineOrderWithOrders(uow, orders, onlineOrder, data);
				}
				
				if(onlineOrder.OnlineOrderStatus == OnlineOrderStatus.Canceled
					&& data.PaymentStatus == OnlineOrderPaymentStatus.Paid)
				{
					onlineOrder.UpdateOnlineOrderPaymentData(
						data.OnlineOrderPaymentType,
						data.OnlinePaymentSource,
						data.PaymentStatus,
						data.UnPaidReason,
						data.OnlinePayment);
					
					SaveOnlineOrder(uow, onlineOrder);
					return Result.Success();
				}
				
				if(onlineOrder.EmployeeWorkWith != null)
				{
					return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.IsOrderAlreadyProcessingAndCannotChanged);
				}
			}
			
			_logger.LogInformation("Полностью обновляем онлайн заказ {OnlineOrderId}", onlineOrder.Id);
			onlineOrder.UpdateOnlineOrder(deliverySchedule, data);

			if(onlineOrder.OnlineOrderStatus == OnlineOrderStatus.New
				&& onlineOrder.EmployeeWorkWith is null
				&& !orders.Any())
			{
				var validationResult = _onlineOrderValidator.ValidateOnlineOrder(uow, onlineOrder, true);

				if(validationResult.IsFailure)
				{
					_logger.LogInformation(
						"Не прошли валидацию онлайн заказа {OnlineOrderId} после его изменения перед оформлением заказа. На ручное...",
						onlineOrder.Id);
					
					SaveOnlineOrder(uow, onlineOrder);
					return Result.Success();
				}
				
				_logger.LogInformation("Проводим заказ после изменения онлайн заказа {OnlineOrderId}", onlineOrder.Id);
				await _orderService.TryCreateOrderFromOnlineOrderAndAcceptAsync(
					uow,
					onlineOrder,
					_routeListService,
					cancellationToken
				);
			}

			SaveOnlineOrder(uow, onlineOrder);
			return Result.Success();
		}

		private Result TryUpdateUnPaidOnlineOrderWithOrders(
			IUnitOfWork uow,
			IEnumerable<Order> orders,
			OnlineOrder onlineOrder,
			UpdateOnlineOrderFromChangeRequest data)
		{
			var needUpdate = true;

			foreach(var order in orders)
			{
				if(_orderRepository.GetOnClosingOrderStatuses().Contains(order.OrderStatus)
					&& (order.PaymentType == PaymentType.Cash
						|| order.PaymentType == PaymentType.DriverApplicationQR
						|| order.PaymentType == PaymentType.SmsQR
						|| order.PaymentType == PaymentType.PaidOnline
						|| order.PaymentType == PaymentType.Terminal)
					&& !order.OnlinePaymentNumber.HasValue)
				{
					needUpdate &= true;
				}
				else
				{
					needUpdate = false;
				}
			}

			if(!needUpdate)
			{
				_logger.LogWarning(
					"Пришел запрос на изменение неоплаченного онлайна {OnlineOrderId} с уже выставленным заказом(ми), бракуем",
					onlineOrder.Id);

				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.IsOrderAlreadyProcessingAndCannotChanged);
			}

			onlineOrder.UpdateOnlineOrderPaymentData(
				data.OnlineOrderPaymentType,
				data.OnlinePaymentSource,
				data.PaymentStatus,
				data.UnPaidReason,
				data.OnlinePayment);

			if(data.OnlinePayment.HasValue
				&& data.PaymentStatus == OnlineOrderPaymentStatus.Paid
				&& data.OnlineOrderPaymentType.HasValue)
			{
				_onlinePaymentAcceptanceHandler.AcceptOnlinePayment(
					uow,
					orders,
					data.OnlinePayment.Value,
					data.OnlineOrderPaymentType.Value.ToOrderPaymentType(),
					uow.GetById<PaymentFrom>(data.OnlinePaymentSource.ConvertToPaymentFromId(_orderSettings)));
			}

			SaveOnlineOrder(uow, onlineOrder);
			return Result.Success();
		}

		private Result TryUpdatePaidOnlineOrder(IUnitOfWork uow, IEnumerable<Order> orders, OnlineOrder onlineOrder, DeliverySchedule deliverySchedule,
			UpdateOnlineOrderFromChangeRequest data)
		{
			_logger.LogWarning("Пришел запрос на изменение оплаченного онлайна {OnlineOrderId}", onlineOrder.Id);
				
			if(orders.Any())
			{
				_logger.LogWarning("Оплаченный онлайн {OnlineOrderId} с уже выставленными заказом(ми), бракуем", onlineOrder.Id);
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.IsOrderAlreadyProcessingAndCannotChanged);
			}

			if(onlineOrder.EmployeeWorkWith != null)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrderErrors.IsOrderAlreadyProcessingAndCannotChanged);
			}
				
			onlineOrder.UpdateOnlineOrderDeliveryData(
				deliverySchedule,
				data.DeliveryScheduleId,
				data.DeliveryDate,
				data.IsFastDelivery);

			SaveOnlineOrder(uow, onlineOrder);
			return Result.Success();
		}

		private void TransferToManualProcessing(
			OnlineOrder onlineOrder,
			TimeSpan timeForTransfer)
		{
			if(onlineOrder.TryMoveToManualProcessingWithoutPaymentByUnPaidReason(
				timeForTransfer.TotalSeconds, "Заказ не был оплачен в отведенный срок"))
			{
				_logger.LogInformation("Переводим на ручное онлайн заказ №{WaitingForPaymentOrderId}", onlineOrder.Id);
			}
		}
		
		private void SaveOnlineOrder(IUnitOfWork uow, OnlineOrder onlineOrder)
		{
			uow.Save(onlineOrder);
			uow.Commit();
		}
	}
}
