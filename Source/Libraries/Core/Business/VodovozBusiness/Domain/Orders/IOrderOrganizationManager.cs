using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public interface IOrderOrganizationManager : IGetOrganizationForOrder
	{
		PartitionedOrderByOrganizations GetOrderPartsByOrganizations(TimeSpan requestTime, Order order, IUnitOfWork uow = null);
	}
}
