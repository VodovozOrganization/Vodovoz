using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts;
using CustomerOrders.Contracts.InfoMessages;
using CustomerOrders.Contracts.V5.Orders;
using CustomerOrders.Contracts.V5.Orders.PromoSets;
using CustomerOrdersApi.Library.Converters;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Extensions;

namespace CustomerOrdersApi.Library.V6.Factories
{
	public class CustomerOrderFactoryV5 : ICustomerOrderFactoryV5
	{
		private readonly IExternalOrderStatusConverter _externalOrderStatusConverter;
		private readonly IInfoMessageFactoryV5 _infoMessageFactory;

		public CustomerOrderFactoryV5(
			IExternalOrderStatusConverter externalOrderStatusConverter,
			IInfoMessageFactoryV5 infoMassageFactory)
		{
			_externalOrderStatusConverter =
				externalOrderStatusConverter ?? throw new ArgumentNullException(nameof(externalOrderStatusConverter));
			_infoMessageFactory = infoMassageFactory ?? throw new ArgumentNullException(nameof(infoMassageFactory));
		}
		
		public DetailedOrderInfoDto CreateDetailedOrderInfo(
			Order order,
			OrderRating orderRating,
			OnlineOrderTimers timers,
			int? onlineOrderId,
			DateTime ratingAvailableFrom)
		{
			var orderInfo = CreateOrderInfoDto(order, timers, orderRating, ratingAvailableFrom, onlineOrderId);
			return orderInfo;
		}

		public DetailedOrderInfoDto CreateDetailedOrderInfo(
			OnlineOrder onlineOrder, OrderRating orderRating, OnlineOrderTimers timers, int? orderId, DateTime ratingAvailableFrom)
		{
			var orderInfo = CreateOrderInfoDto(onlineOrder, timers, orderRating, ratingAvailableFrom, orderId);
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
			OnlineOrderTimers timers,
			OrderRating orderRating,
			DateTime ratingAvailableFrom,
			int? onlineOrderId)
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
				OnlinePaymentType = null
			};

			//при выставленном заказе не нужны сообщения и передача таймера
			orderInfo.InfoMessages = Array.Empty<InfoMessage>();

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
			UpdateOrderRating(orderInfo, orderRating, ratingAvailableFrom);
			UpdateOrderItems(orderInfo, order.OrderItems);

			return orderInfo;
		}
		
		private DetailedOrderInfoDto CreateOrderInfoDto(
			OnlineOrder onlineOrder,
			OnlineOrderTimers timers,
			OrderRating orderRating,
			DateTime ratingAvailableFrom,
			int? orderId)
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
				OnlinePaymentSource = onlineOrder.OnlinePaymentSource.ToExternalPaymentSource(),
				OnlinePaymentType = onlineOrder.OnlineOrderPaymentType.ToExternalOrderPaymentType()
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
			UpdateOrderRating(orderInfo, orderRating, ratingAvailableFrom);
			UpdateOrderItems(orderInfo, onlineOrder.OnlineOrderItems);
			
			return orderInfo;
		}

		private void UpdateAvailabilityRepeatOrder(DetailedOrderInfoDto orderInfo)
		{
			if(orderInfo.OrderStatus is ExternalCustomerOrderStatus.OrderCompleted or ExternalCustomerOrderStatus.Canceled)
			{
				orderInfo.AvailableRepeatOrder = true;
			}
		}
		
		private void UpdateOrderRating(DetailedOrderInfoDto orderInfo, OrderRating orderRating, DateTime ratingAvailableFrom)
		{
			if(orderRating is null)
			{
				orderInfo.IsRatingAvailable =
					orderInfo.CreatedDateTimeUtc >= DateTimeOffset.Parse(ratingAvailableFrom.ToString())
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
			
			var promoSets = new List<OrderPromoSetDto>();

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
					OrderPromoSetDto.Create(
						orderItemGroup.Key,
						orderItemGroup.Count() / promoItemsCount,
						promoPrice
					));
			}

			orderInfo.PromoSets = promoSets;
		}
	}
}
