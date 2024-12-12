using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrdersApi.Library.Converters;
using CustomerOrdersApi.Library.Dto.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Factories
{
	public class CustomerOrderFactory : ICustomerOrderFactory
	{
		private readonly IExternalOrderStatusConverter _externalOrderStatusConverter;

		public CustomerOrderFactory(IExternalOrderStatusConverter externalOrderStatusConverter)
		{
			_externalOrderStatusConverter =
				externalOrderStatusConverter ?? throw new ArgumentNullException(nameof(externalOrderStatusConverter));
		}
		
		public DetailedOrderInfoDto CreateDetailedOrderInfo(
			Order order, OrderRating orderRating, int? onlineOrderId, DateTime ratingAvailableFrom)
		{
			var orderInfo = CreateOrderInfoDto(order, onlineOrderId);
			orderInfo.UpdateOrderRating(orderRating, ratingAvailableFrom);
			orderInfo.UpdateOrderItems(order.OrderItems);
			return orderInfo;
		}

		public DetailedOrderInfoDto CreateDetailedOrderInfo(
			OnlineOrder onlineOrder, OrderRating orderRating, int? orderId, DateTime ratingAvailableFrom)
		{
			var orderInfo = CreateOrderInfoDto(onlineOrder, orderId);
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
		
		private DetailedOrderInfoDto CreateOrderInfoDto(Order order, int? onlineOrderId)
		{
			var orderInfo = new DetailedOrderInfoDto
			{
				OrderId = order.Id,
				OnlineOrderId = onlineOrderId,
				CreationDate = order.CreateDate ?? default,
				DeliveryDate = order.DeliveryDate ?? default,
				IsFastDelivery = order.IsFastDelivery,
				IsSelfDelivery = order.SelfDelivery,
				OrderSum = order.OrderSum,
				OrderStatus = _externalOrderStatusConverter.ConvertOrderStatus(order.OrderStatus)
			};

			if(!order.SelfDelivery)
			{
				orderInfo.DeliveryAddress = order.DeliveryPoint?.ShortAddress;
				orderInfo.DeliverySchedule = orderInfo.IsFastDelivery
					? DeliverySchedule.FastDelivery
					: order.DeliverySchedule?.DeliveryTime;
			}

			return orderInfo;
		}
		
		private DetailedOrderInfoDto CreateOrderInfoDto(OnlineOrder onlineOrder, int? orderId)
		{
			var orderInfo = new DetailedOrderInfoDto
			{
				OrderId = orderId,
				OnlineOrderId = onlineOrder.Id,
				CreationDate = onlineOrder.Created,
				DeliveryDate = onlineOrder.DeliveryDate,
				IsFastDelivery = onlineOrder.IsFastDelivery,
				IsSelfDelivery = onlineOrder.IsSelfDelivery,
				OrderSum = onlineOrder.OnlineOrderSum,
				OrderStatus = _externalOrderStatusConverter.ConvertOnlineOrderStatus(onlineOrder.OnlineOrderStatus)
			};

			if(!onlineOrder.IsSelfDelivery)
			{
				orderInfo.DeliveryAddress = onlineOrder.DeliveryPoint?.ShortAddress;
				orderInfo.DeliverySchedule = orderInfo.IsFastDelivery
					? DeliverySchedule.FastDelivery
					: onlineOrder.DeliverySchedule?.DeliveryTime;
			}

			return orderInfo;
		}
	}
}
