using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Orders
{
	public class OrganizationFromClientForOrderHandler : IGetOrganizationForOrder
	{
		public IReadOnlyDictionary<Organization, IEnumerable<OrderItem>> GetOrganizationsForOrder(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
			=> new Dictionary<Organization, IEnumerable<OrderItem>>
			{
				{ order.Client.WorksThroughOrganization, null }
			};
	}
}
