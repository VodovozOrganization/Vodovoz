using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public interface IGetOrganizationForOrder
	{
		IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow = null);
	}
}
