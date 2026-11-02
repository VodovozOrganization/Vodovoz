using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public class OnlineOrderRepository : IOnlineOrderRepository
	{
	public OnlineOrder GetOnlineOrderByExternalId(IUnitOfWork uow, Guid externalId)
		{
			var onlineOrders = from onlineOrder in uow.Session.Query<OnlineOrder>()
				where onlineOrder.ExternalOrderId == externalId
				select onlineOrder;

			return onlineOrders.FirstOrDefault();
		}
		
		public IEnumerable<OnlineOrder> GetOnlineOrdersDuplicates(
			IUnitOfWork uow, OnlineOrder currentOnlineOrder, DateTime? createdAt = null)
		{
			var onlineOrders = from onlineOrder in uow.Session.Query<OnlineOrder>()
				where onlineOrder.CounterpartyId != null
					&& onlineOrder.CounterpartyId == currentOnlineOrder.CounterpartyId
					&& onlineOrder.DeliveryPointId == currentOnlineOrder.DeliveryPointId
					&& onlineOrder.DeliveryDate == currentOnlineOrder.DeliveryDate
					&& onlineOrder.DeliveryScheduleId == currentOnlineOrder.DeliveryScheduleId
					&& onlineOrder.OnlineOrderSum == currentOnlineOrder.OnlineOrderSum
				select onlineOrder;

			return createdAt.HasValue
				? onlineOrders.Where(o => o.Created.Date >= createdAt.Value.Date).ToList()
				: onlineOrders.ToList();
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
