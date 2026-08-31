using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrdersApi.Library.V7.Dto.Orders;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Repositories
{
	public class CustomerOrderRepository : ICustomerOrderRepository
	{
		public IEnumerable<OrderDto> GetCounterpartyOrdersFromOnlineOrders(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom,
			IEnumerable<ExternalOrderStatus> orderStatuses = null)
		{
			var statusesList = orderStatuses?.ToArray();
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

				let address = order.DeliveryPoint != null ? order.DeliveryPoint.ShortAddress : null
				let deliveryPointId = order.DeliveryPoint != null ? order.DeliveryPoint.Id : (int?)null
				let orderStatus =
					order.OrderStatus == OrderStatus.Canceled
					|| order.OrderStatus == OrderStatus.DeliveryCanceled
					|| order.OrderStatus == OrderStatus.NotDelivered
						? ExternalOrderStatus.Canceled
						: order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList
							? ExternalOrderStatus.OrderPerformed
							: order.OrderStatus == OrderStatus.Shipped
							|| order.OrderStatus == OrderStatus.Closed
							|| order.OrderStatus == OrderStatus.UnloadingOnStock
								? ExternalOrderStatus.OrderCompleted
								: order.OrderStatus == OrderStatus.WaitForPayment
									? ExternalOrderStatus.WaitingForPayment
									: order.OrderStatus == OrderStatus.OnTheWay
										? ExternalOrderStatus.OrderDelivering
										: order.OrderStatus == OrderStatus.OnLoading
											? ExternalOrderStatus.OrderCollecting
											: ExternalOrderStatus.OrderProcessing

				let ratingAvailable =
					order.CreateDate.HasValue
					&& order.CreateDate >= ratingAvailableFrom
					&& orderRating == null
					&& (orderStatus == ExternalOrderStatus.OrderCompleted
						|| orderStatus == ExternalOrderStatus.Canceled
						|| orderStatus == ExternalOrderStatus.OrderDelivering)

				let orderPaymentStatus = order.OnlinePaymentNumber.HasValue
					? OnlineOrderPaymentStatus.Paid
					: OnlineOrderPaymentStatus.UnPaid

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

				let orderSum =
					(uow.Session.Query<OrderItem>()
						.Where(oi => oi.Order.Id == order.Id)
						.Sum(oi => (decimal?)oi.ActualSum) ?? 0m)
					- (uow.Session.Query<OrderDepositItem>()
						.Where(od => od.Order.Id == order.Id)
						.Sum(od => (decimal?)od.ActualSum) ?? 0m)

				where order.Client.Id == counterpartyId

				select new OrderDto
				{
					OrderId = order.Id,
					OnlineOrderId = onlineOrder.Id,
					OrderStatus = orderStatus,
					DeliveryDate = order.DeliveryDate.Value,
					CreatedDateTimeUtc = DateTimeOffset.Parse(order.CreateDate.Value.ToString()),
					OrderSum = orderSum,
					DeliveryAddress = address,
					DeliverySchedule = deliveryScheduleString,
					RatingValue = orderRating.Rating,
					IsRatingAvailable = ratingAvailable,
					IsNeedPay = isNeedPay,
					DeliveryPointId = deliveryPointId
				};

			if(statusesList != null && statusesList.Length > 0)
			{
				orders = orders.Where(x => statusesList.Contains(x.OrderStatus));
			}

			return orders;
		}

		public IEnumerable<OrderDto> GetCounterpartyOrdersWithoutOnlineOrders(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom,
			IEnumerable<ExternalOrderStatus> orderStatuses = null)
		{
			var statusesList = orderStatuses?.ToArray();
			var orders = from order in uow.Session.Query<Order>()
						 join deliverySchedule in uow.Session.Query<DeliverySchedule>()
							 on order.DeliverySchedule.Id equals deliverySchedule.Id into schedules
						 join orderRating in uow.Session.Query<OrderRating>()
							 on order.Id equals orderRating.Order.Id into orderRatings
						 from orderRating in orderRatings.DefaultIfEmpty()
						 from deliverySchedule in schedules.DefaultIfEmpty()

						 let address = order.DeliveryPoint != null ? order.DeliveryPoint.ShortAddress : null
						 let deliveryPointId = order.DeliveryPoint != null ? order.DeliveryPoint.Id : (int?)null
						 let orderStatus =
							 order.OrderStatus == OrderStatus.Canceled
							 || order.OrderStatus == OrderStatus.DeliveryCanceled
							 || order.OrderStatus == OrderStatus.NotDelivered
								 ? ExternalOrderStatus.Canceled
								 : order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList
									 ? ExternalOrderStatus.OrderPerformed
									 : order.OrderStatus == OrderStatus.Shipped
									 || order.OrderStatus == OrderStatus.Closed
									 || order.OrderStatus == OrderStatus.UnloadingOnStock
										 ? ExternalOrderStatus.OrderCompleted
										 : order.OrderStatus == OrderStatus.WaitForPayment
											 ? ExternalOrderStatus.WaitingForPayment
											 : order.OrderStatus == OrderStatus.OnTheWay
												 ? ExternalOrderStatus.OrderDelivering
												 : order.OrderStatus == OrderStatus.OnLoading
													 ? ExternalOrderStatus.OrderCollecting
													 : ExternalOrderStatus.OrderProcessing

						 let ratingAvailable =
							 order.CreateDate.HasValue
							 && order.CreateDate >= ratingAvailableFrom
							 && orderRating == null
							 && (orderStatus == ExternalOrderStatus.OrderCompleted
								 || orderStatus == ExternalOrderStatus.Canceled
								 || orderStatus == ExternalOrderStatus.OrderDelivering)

						 let orderPaymentStatus = order.OnlinePaymentNumber.HasValue
							 ? OnlineOrderPaymentStatus.Paid
							 : OnlineOrderPaymentStatus.UnPaid

						 let deliveryScheduleString = order.IsFastDelivery
							 ? DeliverySchedule.FastDelivery
							 : deliverySchedule != null
								 ? deliverySchedule.DeliveryTime
								 : null

						 let orderSum =
								(uow.Session.Query<OrderItem>()
									.Where(oi => oi.Order.Id == order.Id)
									.Sum(oi => (decimal?)oi.ActualSum) ?? 0m)
								- (uow.Session.Query<OrderDepositItem>()
									.Where(od => od.Order.Id == order.Id)
									.Sum(od => (decimal?)od.ActualSum) ?? 0m)

						 where
							 order.Client.Id == counterpartyId
							 && order.OnlineOrder == null

						 select new OrderDto
						 {
							 OrderId = order.Id,
							 OnlineOrderId = null,
							 OrderStatus = orderStatus,
							 DeliveryDate = order.DeliveryDate != null ? order.DeliveryDate.Value : default,
							 CreatedDateTimeUtc = DateTimeOffset.Parse(order.CreateDate.Value.ToString()),
							 OrderSum = orderSum,
							 DeliveryAddress = address,
							 DeliverySchedule = deliveryScheduleString,
							 RatingValue = orderRating.Rating,
							 IsRatingAvailable = ratingAvailable,
							 IsNeedPay = false,
							 DeliveryPointId = deliveryPointId
						 };

			if(statusesList != null && statusesList.Length > 0)
			{
				orders = orders.Where(x => statusesList.Contains(x.OrderStatus));
			}

			return orders;
		}
		
		public IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(
			IUnitOfWork uow,
			int counterpartyId,
			DateTime ratingAvailableFrom,
			IEnumerable<ExternalOrderStatus> orderStatuses = null)
		{
			var statusesList = orderStatuses?.ToArray();
			var onlineOrders = from onlineOrder in uow.Session.Query<OnlineOrder>()
							   from timer in uow.Session.Query<OnlineOrderTimers>()
							   join orderRating in uow.Session.Query<OrderRating>()
								   on onlineOrder.Id equals orderRating.OnlineOrder.Id into orderRatings
							   from onlineOrderRating in orderRatings.DefaultIfEmpty()

							   let address = onlineOrder.DeliveryPoint != null ? onlineOrder.DeliveryPoint.ShortAddress : null

							   let deliverySchedule =
								   onlineOrder.DeliverySchedule != null && onlineOrder.IsFastDelivery
									   ? DeliverySchedule.FastDelivery
									   : onlineOrder.DeliverySchedule != null
										   ? onlineOrder.DeliverySchedule.DeliveryTime
										   : null

							   let orderStatus =
								   onlineOrder.OnlineOrderStatus == OnlineOrderStatus.OrderPerformed
									   ? ExternalOrderStatus.OrderPerformed
									   : onlineOrder.OnlineOrderStatus == OnlineOrderStatus.Canceled
										   ? ExternalOrderStatus.Canceled
										   : ExternalOrderStatus.OrderProcessing

							   let ratingAvailable =
								   onlineOrder.Created >= ratingAvailableFrom
								   && onlineOrderRating == null
								   && (orderStatus == ExternalOrderStatus.OrderCompleted
									   || orderStatus == ExternalOrderStatus.Canceled
									   || orderStatus == ExternalOrderStatus.OrderDelivering)

							   let payTime = onlineOrder.IsFastDelivery
								   ? (int)timer.PayTimeWithFastDelivery.TotalSeconds
								   : (int)timer.PayTimeWithoutFastDelivery.TotalSeconds

							   let isNeedPay =
								   onlineOrder.OnlineOrderPaymentType == OnlineOrderPaymentType.PaidOnline
								   && onlineOrder.OnlineOrderPaymentStatus != OnlineOrderPaymentStatus.Paid
								   && (DateTime.Now - onlineOrder.Created).TotalSeconds < payTime
							   
							   where
									onlineOrder.Counterparty.Id == counterpartyId
									&& !onlineOrder.Orders.Any()

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

			if(statusesList != null && statusesList.Length > 0)
			{
				onlineOrders = onlineOrders.Where(x => statusesList.Contains(x.OrderStatus));
			}

			return onlineOrders;
		}
	}
}
