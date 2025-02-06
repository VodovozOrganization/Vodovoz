using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Orders
{
	public class OrderOurOrganizationForOrderHandler : IGetOrganizationForOrder
	{
		public IReadOnlyDictionary<Organization, IEnumerable<OrderItem>> GetOrganizationsForOrder(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
			=> new Dictionary<Organization, IEnumerable<OrderItem>>
			{
				{ order.OurOrganization, null }
			};
	}
}
