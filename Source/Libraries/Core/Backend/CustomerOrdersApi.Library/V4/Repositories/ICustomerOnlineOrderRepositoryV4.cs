using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.V4.Orders;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V4.Repositories
{
	public interface ICustomerOnlineOrderRepositoryV4
	{
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
	}
}
