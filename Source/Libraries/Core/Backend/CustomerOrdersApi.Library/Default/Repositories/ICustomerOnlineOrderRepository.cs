using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.Default.Orders;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.Default.Repositories
{
	public interface ICustomerOnlineOrderRepository
	{
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
	}
}
