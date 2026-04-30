using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts;
using CustomerOrdersApi.Library.Converters;
using CustomerOrders.Contracts.Default.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Default.Factories
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
			Order order,
			OrderRating orderRating,
			int? onlineOrderId,
			DateTime ratingAvailableFrom)
		{
			var orderInfo = CreateOrderInfoDto(order, orderRating, ratingAvailableFrom, onlineOrderId);
			return orderInfo;
		}

		public DetailedOrderInfoDto CreateDetailedOrderInfo(
			OnlineOrder onlineOrder, OrderRating orderRating, int? orderId, DateTime ratingAvailableFrom)
		{
			var orderInfo = CreateOrderInfoDto(onlineOrder, orderRating, ratingAvailableFrom, orderId);
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
		
		private DetailedOrderInfoDto CreateOrderInfoDto(
			Order order,
			OrderRating orderRating,
			DateTime ratingAvailableFrom,
			int? onlineOrderId)
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
				var deliveryPoint = order.DeliveryPoint;
				
				if (deliveryPoint != null)
				{
					orderInfo.DeliveryPointId = deliveryPoint.Id;
					orderInfo.DeliveryAddress = deliveryPoint.ShortAddress;
				}
				
				orderInfo.DeliverySchedule = orderInfo.IsFastDelivery
					? DeliverySchedule.FastDelivery
					: order.DeliverySchedule?.DeliveryTime;
			}
			
			UpdateOrderRating(orderInfo, orderRating, ratingAvailableFrom);
			UpdateOrderItems(orderInfo, order.OrderItems);

			return orderInfo;
		}
		
		private DetailedOrderInfoDto CreateOrderInfoDto(
			OnlineOrder onlineOrder,
			OrderRating orderRating,
			DateTime ratingAvailableFrom,
			int? orderId)
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
				var deliveryPoint = onlineOrder.DeliveryPoint;
				
				if (deliveryPoint != null)
				{
					orderInfo.DeliveryPointId = deliveryPoint.Id;
					orderInfo.DeliveryAddress = deliveryPoint.ShortAddress;
				}
				
				orderInfo.DeliverySchedule = orderInfo.IsFastDelivery
					? DeliverySchedule.FastDelivery
					: onlineOrder.DeliverySchedule?.DeliveryTime;
			}

			UpdateOrderRating(orderInfo, orderRating, ratingAvailableFrom);
			UpdateOrderItems(orderInfo, onlineOrder.OnlineOrderItems);
			
			return orderInfo;
		}
		
		private void UpdateOrderRating(DetailedOrderInfoDto orderInfo, OrderRating orderRating, DateTime ratingAvailableFrom)
		{
			if(orderRating is null)
			{
				orderInfo.IsRatingAvailable =
					orderInfo.CreationDate >= ratingAvailableFrom
					&& (orderInfo.OrderStatus == ExternalCustomerOrderStatus.OrderCompleted
						|| orderInfo.OrderStatus == ExternalCustomerOrderStatus.Canceled
						|| orderInfo.OrderStatus == ExternalCustomerOrderStatus.OrderDelivering);
				
				orderInfo.RatingReasonsIds = new List<int>();
				return;
			}

			orderInfo.RatingReasonsIds = orderRating.OrderRatingReasons.Select(x => x.Id).ToList();
			orderInfo.OrderRatingComment = orderRating.Comment;
			orderInfo.RatingValue = orderRating.Rating;
			orderInfo.IsRatingAvailable = false;
		}
		
		private void UpdateOrderItems(DetailedOrderInfoDto orderInfo, IEnumerable<IProduct> orderItems)
		{
			orderInfo.OrderItems = orderItems
				.Where(x => x.PromoSet == null)
				.Select(orderItem =>
					OrderItemDto.Create(
						orderItem.Nomenclature.Id,
						orderItem.CurrentCount,
						orderItem.Price,
						orderItem.IsDiscountInMoney,
						orderItem.GetDiscount))
				.ToList();

			UpdatePromoSets(orderInfo, orderItems);
		}

		private void UpdatePromoSets(DetailedOrderInfoDto orderInfo, IEnumerable<IProduct> orderItems)
		{
			var promoSetsGroup = orderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSet.Id);
			
			var promoSets = new List<PromoSetDto>();

			foreach(var orderItemGroup in promoSetsGroup)
			{
				var promo = orderItemGroup.First().PromoSet;
				var promoItemsCount = promo.PromotionalSetItems.Count;
				var promoPrice = 0m;
				var i = 0;

				foreach(var product in orderItemGroup)
				{
					i++;
					promoPrice += product.ActualSum;

					if(i >= promoItemsCount)
					{
						break;
					}
				}
					
				promoSets.Add(
					PromoSetDto.Create(
						orderItemGroup.Key,
						orderItemGroup.Count() / promoItemsCount,
						promoPrice
					));
			}

			orderInfo.PromoSets = promoSets;
		}
	}
}
