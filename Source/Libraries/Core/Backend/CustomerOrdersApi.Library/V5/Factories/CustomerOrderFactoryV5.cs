using CustomerOrdersApi.Library.Converters;
using CustomerOrdersApi.Library.V5.Dto.Orders;
using CustomerOrdersApi.Library.V5.Services;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Application.Orders.Services;
using Vodovoz.Core.Data.InfoMessages;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using DetailedOrderInfoDto = CustomerOrdersApi.Library.V5.Dto.Orders.DetailedOrderInfoDto;

namespace CustomerOrdersApi.Library.V5.Factories
{
	public class CustomerOrderFactoryV5 : ICustomerOrderFactoryV5
	{
		private readonly IExternalOrderStatusConverter _externalOrderStatusConverter;
		private readonly IInfoMessageFactoryV5 _infoMessageFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly ICustomerOrderCancellationService _orderCancellationLogicService;
		private readonly ICustomerOrderTransferService _orderTransferService;

		public CustomerOrderFactoryV5(
			IExternalOrderStatusConverter externalOrderStatusConverter,
			IInfoMessageFactoryV5 infoMassageFactory,
			IOrderRepository orderRepository,
			ICustomerOrderCancellationService orderCancellationLogicService,
			ICustomerOrderTransferService orderTransferService
			)
		{
			_externalOrderStatusConverter =
				externalOrderStatusConverter ?? throw new ArgumentNullException(nameof(externalOrderStatusConverter));
			_infoMessageFactory = infoMassageFactory ?? throw new ArgumentNullException(nameof(infoMassageFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_orderCancellationLogicService = orderCancellationLogicService ?? throw new ArgumentNullException(nameof(orderCancellationLogicService));
			_orderTransferService = orderTransferService ?? throw new ArgumentNullException(nameof(orderTransferService));
		}

		public async Task<DetailedOrderInfoDto> CreateDetailedOrderInfo(
			IUnitOfWork uow,
			Order order,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			OnlineOrder onlineOrder,
			DateTime ratingAvailableFrom,
			CancellationToken cancellationToken
		)
		{
			var orderInfo = CreateOrderInfoDto(order, timers, onlineOrder?.Id);
			orderInfo.UpdateOrderRating(orderRating, ratingAvailableFrom);
			orderInfo.UpdateOrderItems(order.OrderItems);

			await UpdateAvailableOperations(uow, orderInfo, order, onlineOrder, cancellationToken);

			return orderInfo;
		}

		public async Task<DetailedOrderInfoDto> CreateDetailedOrderInfo(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			int? orderId,
			DateTime ratingAvailableFrom,
			CancellationToken cancellationToken
		)
		{
			var orderInfo = CreateOrderInfoDto(onlineOrder, timers, orderId);
			orderInfo.UpdateOrderRating(orderRating, ratingAvailableFrom);
			orderInfo.UpdateOrderItems(onlineOrder.OnlineOrderItems);

			var activeOrder = GetActiveOrder(onlineOrder);
			await UpdateAvailableOperations(uow, orderInfo, activeOrder, onlineOrder, cancellationToken);

			return orderInfo;
		}

		public IEnumerable<OrderRatingReasonDto> GetOrderRatingReasonDtos(IEnumerable<OrderRatingReason> orderRatingReasons)
		{
			return orderRatingReasons.Select(x => new OrderRatingReasonDto
			{
				OrderRatingReasonId = x.Id,
				Name = x.Name,
				IsArchive = x.IsArchive,
				Ratings = x.GetRatingsArray()
			});
		}

		private DetailedOrderInfoDto CreateOrderInfoDto(Order order, OnlineOrderTimers timers, int? onlineOrderId)
		{
			var orderInfo = new DetailedOrderInfoDto
			{
				OrderId = order.Id,
				OnlineOrderId = onlineOrderId,
				CreatedDateTimeUtc = order.CreateDate.HasValue ? DateTimeOffset.Parse(order.CreateDate.ToString()) : default,
				DeliveryDate = order.DeliveryDate ?? default,
				IsFastDelivery = order.IsFastDelivery,
				IsSelfDelivery = order.SelfDelivery,
				OrderSum = order.OrderSum,
				OrderStatus = _externalOrderStatusConverter.ConvertOrderStatus(order.OrderStatus),
				OnlinePaymentSource = null,
				OnlinePaymentType = null,
				//при выставленном заказе не нужны сообщения и передача таймера
				InfoMessages = Array.Empty<InfoMessage>()
			};

			if(!order.SelfDelivery)
			{
				var deliveryPoint = order.DeliveryPoint;

				if(deliveryPoint != null)
				{
					orderInfo.DeliveryPointId = deliveryPoint.Id;
					orderInfo.DeliveryAddress = deliveryPoint.ShortAddress;
				}

				orderInfo.DeliverySchedule = orderInfo.IsFastDelivery
					? DeliverySchedule.FastDelivery
					: order.DeliverySchedule?.DeliveryTime;
			}

			UpdateAvailabilityRepeatOrder(orderInfo);

			return orderInfo;
		}

		private DetailedOrderInfoDto CreateOrderInfoDto(OnlineOrder onlineOrder, OnlineOrderTimers timers, int? orderId)
		{
			var orderInfo = new DetailedOrderInfoDto
			{
				OrderId = orderId,
				OnlineOrderId = onlineOrder.Id,
				CreatedDateTimeUtc = DateTimeOffset.Parse(onlineOrder.Created.ToString()),
				DeliveryDate = onlineOrder.DeliveryDate,
				IsFastDelivery = onlineOrder.IsFastDelivery,
				IsSelfDelivery = onlineOrder.IsSelfDelivery,
				OrderSum = onlineOrder.OnlineOrderSum,
				OrderStatus = _externalOrderStatusConverter.ConvertOnlineOrderStatus(onlineOrder.OnlineOrderStatus),
				OnlinePaymentSource = onlineOrder.OnlinePaymentSource,
				OnlinePaymentType = onlineOrder.OnlineOrderPaymentType
			};

			if(timers != null)
			{
				var payTime = orderInfo.IsFastDelivery
					? (int)timers.PayTimeWithFastDelivery.TotalSeconds
					: (int)timers.PayTimeWithoutFastDelivery.TotalSeconds;

				var toManualProcessingTime = orderInfo.IsFastDelivery
					? (int)timers.TimeForTransferToManualProcessingWithFastDelivery.TotalSeconds
					: (int)timers.TimeForTransferToManualProcessingWithoutFastDelivery.TotalSeconds;

				if(onlineOrder.IsNeedOnlinePayment(payTime))
				{
					orderInfo.TimerForPaySeconds = payTime;
					orderInfo.IsNeedPay = true;
					orderInfo.InfoMessages = new[] { _infoMessageFactory.CreateNeedPayOrderInfoMessage() };
				}
				else if(onlineOrder.IsNeedOnlinePaymentButTimeIsUp(payTime, toManualProcessingTime))
				{
					orderInfo.InfoMessages = new[] { _infoMessageFactory.CreateNotPaidOrderInfoMessage() };
				}
				else
				{
					orderInfo.InfoMessages = Array.Empty<InfoMessage>();
				}
			}

			if(!onlineOrder.IsSelfDelivery)
			{
				var deliveryPoint = onlineOrder.DeliveryPoint;

				if(deliveryPoint != null)
				{
					orderInfo.DeliveryPointId = deliveryPoint.Id;
					orderInfo.DeliveryAddress = deliveryPoint.ShortAddress;
				}

				orderInfo.DeliverySchedule = orderInfo.IsFastDelivery
					? DeliverySchedule.FastDelivery
					: onlineOrder.DeliverySchedule?.DeliveryTime;
			}

			UpdateAvailabilityRepeatOrder(orderInfo);

			return orderInfo;
		}

		private void UpdateAvailabilityRepeatOrder(DetailedOrderInfoDto orderInfo)
		{
			if(orderInfo.OrderStatus is ExternalOrderStatus.OrderCompleted or ExternalOrderStatus.Canceled)
			{
				orderInfo.AvailableRepeatOrder = true;
			}
		}

		/// <summary>
		/// Получает активный заказ из онлайн-заказа
		/// </summary>
		private Order GetActiveOrder(OnlineOrder onlineOrder)
		{
			var availableStatuses = _orderRepository.GetStatusesForTransferOrCancellationOnlineOrder();
			return onlineOrder.Orders?.FirstOrDefault(x => availableStatuses.Contains(x.OrderStatus));
		}

		/// <summary>
		/// Обновляет доступность операций (отмена, перенос) и добавляет информационные сообщения
		/// </summary>
		private async Task UpdateAvailableOperations(
			IUnitOfWork uow,
			DetailedOrderInfoDto orderInfo,
			Order order,
			OnlineOrder onlineOrder,
			CancellationToken cancellationToken
		)
		{
			var cancelResult = await _orderCancellationLogicService.CanCancel(
				uow, 
				order, 
				onlineOrder, 
				cancellationToken
			);

			orderInfo.AvailableCancelOrder = cancelResult.IsSuccess;

			if(orderInfo.AvailableCancelOrder && (order is not null || onlineOrder is not null))
			{
				AddCancelOrderInfoMessage(orderInfo, order, onlineOrder);
			}

			if(order is not null)
			{
				var transferResult = _orderTransferService.CanTransfer(order);
				orderInfo.AvailableChangeDeliverySchedule = transferResult.IsSuccess;
			}
			else
			{
				orderInfo.AvailableChangeDeliverySchedule = false;
			}
		}

		/// <summary>
		/// Добавляет информационное сообщение об отмене для оплаченных заказов
		/// </summary>
		private void AddCancelOrderInfoMessage(
			DetailedOrderInfoDto orderInfo,
			Order order,
			OnlineOrder onlineOrder)
		{
			var isPaid = false;

			if(order is not null)
			{
				isPaid = order.PaymentType is PaymentType.PaidOnline;
			}
			else if(onlineOrder is not null)
			{
				isPaid = onlineOrder.OnlineOrderPaymentStatus is OnlineOrderPaymentStatus.Paid;
			}

			if(isPaid)
			{
				var existingMessages = orderInfo.InfoMessages?.ToList() ?? new List<InfoMessage>();
				existingMessages.Add(_infoMessageFactory.CreateRefundPaymentInfoMessage());
				orderInfo.InfoMessages = existingMessages;
			}
		}
	}
}
