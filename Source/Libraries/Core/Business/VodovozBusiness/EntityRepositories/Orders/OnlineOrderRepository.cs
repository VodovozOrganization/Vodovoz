﻿using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Data.Orders.Default;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public class OnlineOrderRepository : IOnlineOrderRepository
	{
		public IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(
			IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom)
		{
			var onlineOrders = from onlineOrder in uow.Session.Query<OnlineOrder>()
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

				select new OrderDto
				{
					OnlineOrderId = onlineOrder.Id,
					DeliveryDate = onlineOrder.DeliveryDate,
					CreationDate = onlineOrder.Created,
					OrderStatus = orderStatus,
					DeliveryAddress = address,
					OrderSum = onlineOrder.OnlineOrderSum,
					DeliverySchedule = deliverySchedule,
					RatingValue = onlineOrderRating.Rating,
					IsRatingAvailable = ratingAvailable,
					IsNeedPayment = false,
					DeliveryPointId = onlineOrder.DeliveryPointId
				};

			return onlineOrders;
		}
		
		public IEnumerable<Vodovoz.Core.Data.Orders.V4.OrderDto> GetCounterpartyOnlineOrdersWithoutOrderV4(
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

				let isNeedPay =
					onlineOrder.OnlineOrderStatus == OnlineOrderStatus.WaitingForPayment
					|| (onlineOrder.IsFastDelivery
						? (DateTime.Now - onlineOrder.Created).TotalSeconds < timer.PayTimeWithFastDelivery.TotalSeconds
						: (DateTime.Now - onlineOrder.Created).TotalSeconds < timer.PayTimeWithoutFastDelivery.TotalSeconds)

				select new Vodovoz.Core.Data.Orders.V4.OrderDto
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
		
		public OnlineOrder GetOnlineOrderByExternalId(IUnitOfWork uow, Guid externalId)
		{
			var onlineOrders = from onlineOrder in uow.Session.Query<OnlineOrder>()
				where onlineOrder.ExternalOrderId == externalId
				select onlineOrder;

			return onlineOrders.FirstOrDefault();
		}

		public OnlineOrder GetOnlineOrderById(IUnitOfWork uow, int onlineOrderId)
		{
			var onlineOrders = from onlineOrder in uow.Session.Query<OnlineOrder>()
				where onlineOrder.Id == onlineOrderId
				select onlineOrder;

			return onlineOrders.FirstOrDefault();
		}

		public IEnumerable<OnlineOrder> GetWaitingForPaymentOnlineOrders(IUnitOfWork uow)
		{
			var onlineOrders = from onlineOrder in uow.Session.Query<OnlineOrder>()
				where onlineOrder.OnlineOrderStatus == OnlineOrderStatus.WaitingForPayment
				select onlineOrder;

			return onlineOrders.ToList();
		}
	}
}
