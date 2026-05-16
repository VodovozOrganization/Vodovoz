using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.V4.Orders;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V4.Repositories
{
	public interface ICustomerOrderRepositoryV4
	{
		IEnumerable<OrderDto> GetCounterpartyOrdersFromOnlineOrders(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
		IEnumerable<OrderDto> GetCounterpartyOrdersWithoutOnlineOrders(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
	}
}
