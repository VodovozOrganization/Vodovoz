using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrganizationFromClientForOrderHandler : IGetOrganizationForOrder
	{
		public IEnumerable<OrganizationForOrderWithOrderItems> GetOrganizationsWithOrderItems(
			Order order,
			IUnitOfWork uow = null)
			=> new List<OrganizationForOrderWithOrderItems>
			{
				new OrganizationForOrderWithOrderItems(order.Client.WorksThroughOrganization)
			};
	}
}
