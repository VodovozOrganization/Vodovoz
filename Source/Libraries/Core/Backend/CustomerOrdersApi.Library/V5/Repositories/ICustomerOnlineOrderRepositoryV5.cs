using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V5.Repositories
{
	public interface ICustomerOnlineOrderRepositoryV5
	{
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
	}
}
