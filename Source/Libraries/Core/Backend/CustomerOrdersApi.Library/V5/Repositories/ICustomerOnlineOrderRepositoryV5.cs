using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.V5.Dto.Orders;
using QS.DomainModel.UoW;

namespace CustomerOrdersApi.Library.V5.Repositories
{
	public interface ICustomerOnlineOrderRepositoryV5
	{
		IEnumerable<OrderDto> GetCounterpartyOnlineOrdersWithoutOrder(IUnitOfWork uow, int counterpartyId, DateTime ratingAvailableFrom);
	}
}
