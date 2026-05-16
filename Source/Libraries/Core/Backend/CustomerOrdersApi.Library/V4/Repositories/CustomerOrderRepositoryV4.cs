using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts;
using CustomerOrders.Contracts.V4.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V4.Repositories
{
	public class CustomerOrderRepositoryV4 : ICustomerOrderRepositoryV4
	{
		public IEnumerable<OrderDto> GetCounterpartyOrdersFromOnlineOrders(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom)
	{
		var orders =
			from onlineOrder in uow.Session.Query<OnlineOrder>()
			from timer in uow.Session.Query<OnlineOrderTimers>()
			join order in uow.Session.Query<Order>()
				on onlineOrder.Id equals order.OnlineOrder.Id
			join deliverySchedule in uow.Session.Query<DeliverySchedule>()
				on order.DeliverySchedule.Id equals deliverySchedule.Id into schedules
			join orderRating in uow.Session.Query<OrderRating>()
				on onlineOrder.Id equals orderRating.OnlineOrder.Id into orderRatings
			from orderRating in orderRatings.DefaultIfEmpty()
			from deliverySchedule in schedules.DefaultIfEmpty()
			where order.Client.Id == counterpartyId
			let address = order.DeliveryPoint != null ? order.DeliveryPoint.ShortAddress : null
			let deliveryPointId = order.DeliveryPoint != null ? order.DeliveryPoint.Id : (int?)null
			let orderStatus =
				order.OrderStatus == OrderStatus.Canceled
				|| order.OrderStatus == OrderStatus.DeliveryCanceled
				|| order.OrderStatus == OrderStatus.NotDelivered
					? ExternalCustomerOrderStatus.Canceled
					: order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList
						? ExternalCustomerOrderStatus.OrderPerformed
						: order.OrderStatus == OrderStatus.Shipped
						|| order.OrderStatus == OrderStatus.Closed
						|| order.OrderStatus == OrderStatus.UnloadingOnStock
							? ExternalCustomerOrderStatus.OrderCompleted
							: order.OrderStatus == OrderStatus.WaitForPayment
								? ExternalCustomerOrderStatus.WaitingForPayment
								: order.OrderStatus == OrderStatus.OnTheWay
									? ExternalCustomerOrderStatus.OrderDelivering
									: order.OrderStatus == OrderStatus.OnLoading
										? ExternalCustomerOrderStatus.OrderCollecting
										: ExternalCustomerOrderStatus.OrderProcessing

			let ratingAvailable =
				order.CreateDate.HasValue
				&& order.CreateDate >= ratingAvailableFrom
				&& orderRating == null
				&& (orderStatus == ExternalCustomerOrderStatus.OrderCompleted
					|| orderStatus == ExternalCustomerOrderStatus.Canceled
					|| orderStatus == ExternalCustomerOrderStatus.OrderDelivering)

			let orderPaymentStatus = order.OnlinePaymentNumber.HasValue
				? ExternalOrderPaymentStatus.Paid
				: ExternalOrderPaymentStatus.UnPaid

			let deliveryScheduleString = order.IsFastDelivery
				? DeliverySchedule.FastDelivery
				: deliverySchedule != null
					? deliverySchedule.DeliveryTime
					: null

			let isNeedPay =
				onlineOrder.OnlineOrderStatus == OnlineOrderStatus.WaitingForPayment
				&& onlineOrder.OnlineOrderPaymentType == OnlineOrderPaymentType.PaidOnline
				&& onlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.UnPaid
				&& (order.IsFastDelivery
					? (DateTime.Now - onlineOrder.Created).TotalSeconds < timer.PayTimeWithFastDelivery.TotalSeconds
					: (DateTime.Now - onlineOrder.Created).TotalSeconds < timer.PayTimeWithoutFastDelivery.TotalSeconds)

			select new OrderDto
			{
				OrderId = order.Id,
				OnlineOrderId = onlineOrder.Id,
				OrderStatus = orderStatus,
				DeliveryDate = order.DeliveryDate.Value,
				CreatedDateTimeUtc = DateTimeOffset.Parse(order.CreateDate.Value.ToString()),
				OrderSum = order.OrderSum,
				DeliveryAddress = address,
				DeliverySchedule = deliveryScheduleString,
				RatingValue = orderRating.Rating,
				IsRatingAvailable = ratingAvailable,
				IsNeedPay = isNeedPay,
				DeliveryPointId = deliveryPointId
			};

		return orders;
	}

	public IEnumerable<OrderDto> GetCounterpartyOrdersWithoutOnlineOrders(
		IUnitOfWork uow,
		int counterpartyId,
		DateTime ratingAvailableFrom)
	{
		var orders = from order in uow.Session.Query<Order>()
			join deliverySchedule in uow.Session.Query<DeliverySchedule>()
				on order.DeliverySchedule.Id equals deliverySchedule.Id into schedules
			join orderRating in uow.Session.Query<OrderRating>()
				on order.Id equals orderRating.Order.Id into orderRatings
			from orderRating in orderRatings.DefaultIfEmpty()
			from deliverySchedule in schedules.DefaultIfEmpty()
			where order.Client.Id == counterpartyId && order.OnlineOrder == null
			let address = order.DeliveryPoint != null ? order.DeliveryPoint.ShortAddress : null
			let deliveryPointId = order.DeliveryPoint != null ? order.DeliveryPoint.Id : (int?)null
			let orderStatus =
				order.OrderStatus == OrderStatus.Canceled
				|| order.OrderStatus == OrderStatus.DeliveryCanceled
				|| order.OrderStatus == OrderStatus.NotDelivered
					? ExternalCustomerOrderStatus.Canceled
					: order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList
						? ExternalCustomerOrderStatus.OrderPerformed
						: order.OrderStatus == OrderStatus.Shipped
						|| order.OrderStatus == OrderStatus.Closed
						|| order.OrderStatus == OrderStatus.UnloadingOnStock
							? ExternalCustomerOrderStatus.OrderCompleted
							: order.OrderStatus == OrderStatus.WaitForPayment
								? ExternalCustomerOrderStatus.WaitingForPayment
								: order.OrderStatus == OrderStatus.OnTheWay
									? ExternalCustomerOrderStatus.OrderDelivering
									: order.OrderStatus == OrderStatus.OnLoading
										? ExternalCustomerOrderStatus.OrderCollecting
										: ExternalCustomerOrderStatus.OrderProcessing

			let ratingAvailable =
				order.CreateDate.HasValue
				&& order.CreateDate >= ratingAvailableFrom
				&& orderRating == null
				&& (orderStatus == ExternalCustomerOrderStatus.OrderCompleted
					|| orderStatus == ExternalCustomerOrderStatus.Canceled
					|| orderStatus == ExternalCustomerOrderStatus.OrderDelivering)

			let orderPaymentStatus = order.OnlinePaymentNumber.HasValue
				? ExternalOrderPaymentStatus.Paid
				: ExternalOrderPaymentStatus.UnPaid

			let deliveryScheduleString = order.IsFastDelivery
				? DeliverySchedule.FastDelivery
				: deliverySchedule != null
					? deliverySchedule.DeliveryTime
					: null

			select new OrderDto
			{
				OrderId = order.Id,
				OnlineOrderId = null,
				OrderStatus = orderStatus,
				DeliveryDate = order.DeliveryDate.Value,
				CreatedDateTimeUtc = DateTimeOffset.Parse(order.CreateDate.Value.ToString()),
				OrderSum = order.OrderSum,
				DeliveryAddress = address,
				DeliverySchedule = deliveryScheduleString,
				RatingValue = orderRating.Rating,
				IsRatingAvailable = ratingAvailable,
				IsNeedPay = false,
				DeliveryPointId = deliveryPointId
			};

		return orders;
	}
	}
}
