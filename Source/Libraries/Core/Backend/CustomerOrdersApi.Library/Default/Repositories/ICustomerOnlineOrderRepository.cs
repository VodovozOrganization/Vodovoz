using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.Default.Dto.Orders;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.Default.Repositories
{
	public interface ICustomerOnlineOrderRepository
	{
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
	}
}
