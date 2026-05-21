using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts;
using CustomerOrders.Contracts.V5.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V6.Repositories
{
	public class CustomerOnlineOrderRepositoryV5 : ICustomerOnlineOrderRepositoryV5
	{
		public IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(
			IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom)
		{
			var onlineOrders = from onlineOrder in uow.Session.Query<OnlineOrder>()
				from timer in uow.Session.Query<OnlineOrderTimers>()
				join orderRating in uow.Session.Query<OrderRating>()
					on onlineOrder.Id equals orderRating.OnlineOrder.Id into orderRatings
				from onlineOrderRating in orderRatings.DefaultIfEmpty()
				where onlineOrder.Counterparty.Id == counterpartyId
					&& !onlineOrder.Orders.Any()

				let address = onlineOrder.DeliveryPoint != null ? onlineOrder.DeliveryPoint.ShortAddress : null

				let deliverySchedule =
					onlineOrder.DeliverySchedule != null && onlineOrder.IsFastDelivery
						? DeliverySchedule.FastDelivery
						: onlineOrder.DeliverySchedule != null 
							? onlineOrder.DeliverySchedule.DeliveryTime
							: null

				let orderStatus =
					onlineOrder.OnlineOrderStatus == OnlineOrderStatus.OrderPerformed
						? ExternalCustomerOrderStatus.OrderPerformed
						: onlineOrder.OnlineOrderStatus == OnlineOrderStatus.Canceled
							? ExternalCustomerOrderStatus.Canceled
							: ExternalCustomerOrderStatus.OrderProcessing

				let ratingAvailable =
					onlineOrder.Created >= ratingAvailableFrom
					&& onlineOrderRating == null
					&& (orderStatus == ExternalCustomerOrderStatus.OrderCompleted
						|| orderStatus == ExternalCustomerOrderStatus.Canceled
						|| orderStatus == ExternalCustomerOrderStatus.OrderDelivering)

				let payTime = onlineOrder.IsFastDelivery
					? (int)timer.PayTimeWithFastDelivery.TotalSeconds
					: (int)timer.PayTimeWithoutFastDelivery.TotalSeconds

				let isNeedPay =
					onlineOrder.OnlineOrderPaymentType == OnlineOrderPaymentType.PaidOnline
					&& onlineOrder.OnlineOrderPaymentStatus != OnlineOrderPaymentStatus.Paid
					&& (DateTime.Now - onlineOrder.Created).TotalSeconds < payTime

				select new OrderDto
				{
					OnlineOrderId = onlineOrder.Id,
					DeliveryDate = onlineOrder.DeliveryDate,
					CreatedDateTimeUtc = onlineOrder.Created.ToUniversalTime(),
					OrderStatus = orderStatus,
					DeliveryAddress = address,
					OrderSum = onlineOrder.OnlineOrderSum,
					DeliverySchedule = deliverySchedule,
					RatingValue = onlineOrderRating.Rating,
					IsRatingAvailable = ratingAvailable,
					IsNeedPay = isNeedPay,
					DeliveryPointId = onlineOrder.DeliveryPointId
				};

			return onlineOrders;
		}
	}
}
