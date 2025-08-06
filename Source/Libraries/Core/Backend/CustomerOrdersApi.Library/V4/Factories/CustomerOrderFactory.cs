﻿using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrdersApi.Library.Converters;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using Vodovoz.Core.Data.InfoMessages;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V4.Factories
{
	public class CustomerOrderFactory : ICustomerOrderFactory
	{
		private readonly IExternalOrderStatusConverter _externalOrderStatusConverter;
		private readonly IInfoMassageFactory _infoMassageFactory;

		public CustomerOrderFactory(
			IExternalOrderStatusConverter externalOrderStatusConverter,
			IInfoMassageFactory infoMassageFactory)
		{
			_externalOrderStatusConverter =
				externalOrderStatusConverter ?? throw new ArgumentNullException(nameof(externalOrderStatusConverter));
			_infoMassageFactory = infoMassageFactory ?? throw new ArgumentNullException(nameof(infoMassageFactory));
		}
		
		public DetailedOrderInfoDto CreateDetailedOrderInfo(
			Order order,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			int? onlineOrderId,
			DateTime ratingAvailableFrom)
		{
			var orderInfo = CreateOrderInfoDto(order, timers, onlineOrderId);
			orderInfo.UpdateOrderRating(orderRating, ratingAvailableFrom);
			orderInfo.UpdateOrderItems(order.OrderItems);
			return orderInfo;
		}

		public DetailedOrderInfoDto CreateDetailedOrderInfo(
			OnlineOrder onlineOrder, OrderRating orderRating, OnlineOrderTimers timers, int? orderId, DateTime ratingAvailableFrom)
		{
			var orderInfo = CreateOrderInfoDto(onlineOrder, timers, orderId);
			orderInfo.UpdateOrderRating(orderRating, ratingAvailableFrom);
			orderInfo.UpdateOrderItems(onlineOrder.OnlineOrderItems);
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
			};

			//при выставленном заказе не нужны сообщения и передача таймера
			orderInfo.InfoMessages = Array.Empty<InfoMessage>();

			if(!order.SelfDelivery)
			{
				orderInfo.DeliveryAddress = order.DeliveryPoint?.ShortAddress;
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
				OrderStatus = _externalOrderStatusConverter.ConvertOnlineOrderStatus(onlineOrder.OnlineOrderStatus)
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
					orderInfo.InfoMessages = new[] { _infoMassageFactory.CreateNeedPayOrderInfoMessage() };
				}
				else if(onlineOrder.IsNeedOnlinePaymentButTimeIsUp(payTime, toManualProcessingTime))
				{
					orderInfo.InfoMessages = new[] { _infoMassageFactory.CreateNotPaidOrderInfoMessage() };
				}
				else
				{
					orderInfo.InfoMessages = Array.Empty<InfoMessage>();
				}
			}

			if(!onlineOrder.IsSelfDelivery)
			{
				orderInfo.DeliveryAddress = onlineOrder.DeliveryPoint?.ShortAddress;
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
	}
}
