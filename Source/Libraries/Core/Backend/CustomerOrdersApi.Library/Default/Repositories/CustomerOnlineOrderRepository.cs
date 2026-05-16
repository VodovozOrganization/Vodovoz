using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrders.Contracts;
using CustomerOrders.Contracts.Default.Orders;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Default.Repositories
{
	public class CustomerOnlineOrderRepository : ICustomerOnlineOrderRepository
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

		public async Task<bool> IsClientHasNotCancelledOnlineOrdersFromSource(
			IUnitOfWork uow,
			Guid externalCounterpartyId,
			int? counterpartyErpId,
			Source source,
			CancellationToken cancellationToken = default)
		{
			var orderNotDeliveredStatuses = new[] { OrderStatus.Canceled, OrderStatus.DeliveryCanceled, OrderStatus.NotDelivered };

			var query =
				from onlineOrder in uow.Session.Query<OnlineOrder>()
				join o in uow.Session.Query<Order>() on onlineOrder.Id equals o.OnlineOrder.Id into orders
				from order in orders.DefaultIfEmpty()

				where
				onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled
				&& (order.Id == null || !orderNotDeliveredStatuses.Contains(order.OrderStatus))
				&& onlineOrder.Source == source
				&& (onlineOrder.ExternalCounterpartyId == externalCounterpartyId
					|| (counterpartyErpId != null && order.Client.Id == counterpartyErpId.Value))

				select onlineOrder;

			var firstNotCancelledOnlineOrder = await query.FirstOrDefaultAsync(cancellationToken);

			return firstNotCancelledOnlineOrder != null;
		}
	}
}
