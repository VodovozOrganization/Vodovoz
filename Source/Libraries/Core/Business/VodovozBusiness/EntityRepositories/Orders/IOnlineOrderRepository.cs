﻿using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.Default;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface IOnlineOrderRepository
	{
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		IEnumerable<Vodovoz.Core.Data.Orders.V4.OrderDto> GetCounterpartyOnlineOrdersWithoutOrderV4(
			IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		OnlineOrder GetOnlineOrderByExternalId(IUnitOfWork uow, Guid externalId);
		OnlineOrder GetOnlineOrderById(IUnitOfWork uow, int onlineOrderId);
		IEnumerable<OnlineOrder> GetWaitingForPaymentOnlineOrders(IUnitOfWork uow);
	}
}
