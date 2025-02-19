using System.Collections.Generic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Orders
{
	public class OrganizationForOrderWithOrderItems
	{
		public OrganizationForOrderWithOrderItems(
			Organization organization,
			IEnumerable<OrderItem> orderItems = null)
		{
			Organization = organization;
			OrderItems = orderItems;
		}
		
		public Organization Organization { get; }
		public IEnumerable<OrderItem> OrderItems { get; }
	}
}
