using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public interface IGetOrganizationForOrder
	{
		IEnumerable<PartOrderWithGoods> GetOrganizationsWithOrderItems(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice
			);
	}
}
