using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public interface IGetOrganizationForOrder
	{
		IEnumerable<OrganizationForOrderWithOrderItems> GetOrganizationsWithOrderItems(
			Order order,
			IUnitOfWork uow = null);
	}
}
