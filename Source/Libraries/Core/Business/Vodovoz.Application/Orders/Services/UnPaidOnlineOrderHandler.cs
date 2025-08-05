using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Extensions;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class UnPaidOnlineOrderHandler : IUnPaidOnlineOrderHandler
	{
		private readonly ILogger<UnPaidOnlineOrderHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IOrderSettings _orderSettings;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderOnlinePaymentAcceptanceHandler _onlinePaymentAcceptanceHandler;

		public UnPaidOnlineOrderHandler(
			ILogger<UnPaidOnlineOrderHandler> logger,
			IUnitOfWork uow,
			IOrderSettings orderSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOrderRepository orderRepository,
			IOrderOnlinePaymentAcceptanceHandler onlinePaymentAcceptanceHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_onlinePaymentAcceptanceHandler =
				onlinePaymentAcceptanceHandler ?? throw new ArgumentNullException(nameof(onlinePaymentAcceptanceHandler));
		}

		public async Task TryMoveToManualProcessingWaitingForPaymentOnlineOrders()
		{
			_logger.LogInformation("Проверяем онлайн заказы, ожидающих оплаты...");

			try
			{
				var waitingForPaymentOnlineOrders = _onlineOrderRepository.GetWaitingForPaymentOnlineOrders(_uow);
				_logger.LogInformation("Найдено {WaitingForPaymentCount} онлайн заказов", waitingForPaymentOnlineOrders.Count());

				if(!waitingForPaymentOnlineOrders.Any())
				{
					return;
				}

				var onlineOrderTimers = _uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

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

					await _uow.SaveAsync(onlineOrder);
				}

				_logger.LogInformation("Сохраняем обработанные заказы");
				await _uow.CommitAsync();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке онлайн заказов, ожидающих оплаты");
			}
		}

		public Result CanChangePaymentType(OnlineOrder onlineOrder)
		{
			if(onlineOrder is null)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.OnlineOrderNotFound);
			}

			if(onlineOrder.OnlineOrderStatus != OnlineOrderStatus.WaitingForPayment)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.IsOnlineOrderNotWaitForPayment);
			}

			var onlineOrderTimers = _uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

			if(onlineOrderTimers is null)
			{
				_logger.LogWarning("Не найдены таймеры онлайн заказов");
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.IsOnlineOrderTimersEmpty);
			}

			var timeForPay = onlineOrder.IsFastDelivery
				? onlineOrderTimers.PayTimeWithFastDelivery
				: onlineOrderTimers.PayTimeWithoutFastDelivery;

			if((DateTime.Now - onlineOrder.Created).TotalSeconds >= timeForPay.TotalSeconds)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.HasTimeToPayOrderExpired);
			}
			
			if(onlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.Paid)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.IsOnlineOrderPaid);
			}

			if(onlineOrder.OnlineOrderPaymentType != OnlineOrderPaymentType.PaidOnline)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.CantChangePaymentType);
			}
			
			return Result.Success();
		}

		public Result TryUpdateOrder(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			Domain.Logistic.DeliverySchedule deliverySchedule,
			UpdateOnlineOrderFromChangeRequest data)
		{
			if(onlineOrder is null)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.OnlineOrderNotFound);
			}
			
			if(data.PaymentStatus == OnlineOrderPaymentStatus.Paid && !data.OnlinePayment.HasValue)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.OnlineOrderIsPaidButOnlinePaymentIsEmpty);
			}
			
			if(data.DeliveryScheduleId.HasValue && deliverySchedule is null)
			{
				return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.IsUnknownDeliverySchedule);
			}
			
			//у оплаченных мы меняем только информацию по доставке в нужных состояниях
			if(onlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.Paid)
			{
				_logger.LogWarning("Пришел запрос на изменение оплаченного онлайна {OnlineOrderId}", onlineOrder.Id);
				
				if(order != null)
				{
					_logger.LogWarning(
						"Оплаченный онлайн {OnlineOrderId} с уже выставленным заказом {OrderId}, обрабатываем",
						onlineOrder.Id,
						order.Id);
					
					if(order.OrderStatus == OrderStatus.NewOrder || order.OrderStatus == OrderStatus.Accepted)
					{
						onlineOrder.UpdateOnlineOrder(deliverySchedule, data.DeliveryScheduleId, data.DeliveryDate);
						//обновление заказа
						SaveOrders(uow, onlineOrder, order);
						return Result.Success();
					}
					
					return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.IsOrderAlreadyProcessingAndCannotChanged);
				}

				if(onlineOrder.EmployeeWorkWith != null)
				{
					return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.IsOrderAlreadyProcessingAndCannotChanged);
				}
				
				onlineOrder.UpdateOnlineOrder(deliverySchedule, data.DeliveryScheduleId, data.DeliveryDate);
				SaveOrders(uow, onlineOrder);
				return Result.Success();
			}
			else
			{
				if(order != null)
				{
					if(_orderRepository.GetOnClosingOrderStatuses().Contains(order.OrderStatus)
						&& (order.PaymentType == PaymentType.Cash
							|| order.PaymentType == PaymentType.DriverApplicationQR
							|| order.PaymentType == PaymentType.SmsQR
							|| order.PaymentType == PaymentType.PaidOnline
							|| order.PaymentType == PaymentType.Terminal)
						&& !order.OnlinePaymentNumber.HasValue)
					{
						onlineOrder.UpdateOnlineOrder(
							data.OnlineOrderPaymentType,
							data.OnlinePaymentSource,
							data.PaymentStatus,
							data.UnPaidReason,
							data.OnlinePayment);

						if(data.OnlinePayment.HasValue && data.PaymentStatus == OnlineOrderPaymentStatus.Paid)
						{
							_onlinePaymentAcceptanceHandler.AcceptOnlinePayment(
								uow,
								order,
								data.OnlinePayment.Value,
								data.OnlineOrderPaymentType.ToOrderPaymentType(),
								uow.GetById<PaymentFrom>(data.OnlinePaymentSource.ConvertToPaymentFromId(_orderSettings)));
						}

						SaveOrders(uow, onlineOrder);
						return Result.Success();
					}
					
					_logger.LogWarning(
						"Пришел запрос на изменение неоплаченного онлайна {OnlineOrderId} с уже выставленным заказом {OrderId}, бракуем",
						onlineOrder.Id,
						order.Id);
					
					return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.IsOrderAlreadyProcessingAndCannotChanged);
				}
				
				if(onlineOrder.OnlineOrderStatus == OnlineOrderStatus.Canceled
					&& data.PaymentStatus == OnlineOrderPaymentStatus.Paid)
				{
					onlineOrder.UpdateOnlineOrder(
						data.OnlineOrderPaymentType,
						data.OnlinePaymentSource,
						data.PaymentStatus,
						data.UnPaidReason,
						data.OnlinePayment);
					
					SaveOrders(uow, onlineOrder);
					return Result.Success();
				}
				
				if(onlineOrder.EmployeeWorkWith != null)
				{
					return Result.Failure(Vodovoz.Errors.Orders.OnlineOrder.IsOrderAlreadyProcessingAndCannotChanged);
				}
			}
			
			_logger.LogInformation("Полностью обновляем онлайн {OnlineOrderId}", onlineOrder.Id);
			onlineOrder.UpdateOnlineOrder(deliverySchedule, data);
			SaveOrders(uow, onlineOrder);
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
		
		private void SaveOrders(IUnitOfWork uow, OnlineOrder onlineOrder, Order order = null)
		{
			if(order != null)
			{
				uow.Save(order);
			}
			
			uow.Save(onlineOrder);
			uow.Commit();
		}
	}
}
