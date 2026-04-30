using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.Default.Orders;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.Default.Repositories
{
	public interface ICustomerOrderRepository
	{
		IEnumerable<OrderDto> GetCounterpartyOrdersFromOnlineOrders(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		IEnumerable<OrderDto> GetCounterpartyOrdersWithoutOnlineOrders(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
	}
}
