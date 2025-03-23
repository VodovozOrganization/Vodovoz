using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public interface IOrderOrganizationManager : IGetOrganizationForOrder
	{
		OrderForOrderWithGoodsEquipmentsAndDeposits GetOrderPartsByOrganizations(TimeSpan requestTime, Order order, IUnitOfWork uow = null);
	}
}
