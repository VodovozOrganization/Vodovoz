using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface IOnlineOrderRepository
	{
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		OnlineOrder GetOnlineOrderByExternalId(IUnitOfWork uow, Guid externalId);
		IEnumerable<OnlineOrder> GetOnlineOrdersDuplicates(IUnitOfWork uow, OnlineOrder currentOnlineOrder, DateTime? createdAt = null);
	}
}
