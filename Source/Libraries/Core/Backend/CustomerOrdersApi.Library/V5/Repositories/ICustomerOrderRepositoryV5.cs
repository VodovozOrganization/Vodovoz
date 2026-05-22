using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V5.Repositories
{
	public interface ICustomerOrderRepositoryV5
	{
		IEnumerable<OrderDto> GetCounterpartyOrdersFromOnlineOrders(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		IEnumerable<OrderDto> GetCounterpartyOrdersWithoutOnlineOrders(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
	}
}
