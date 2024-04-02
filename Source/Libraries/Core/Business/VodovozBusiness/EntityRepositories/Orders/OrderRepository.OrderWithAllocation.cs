using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Orders
{
	public partial class OrderRepository
	{
		public class OrderWithAllocation
		{
			public int OrderId { get; set; }
			public DateTime OrderDeliveryDate { get; set; }
			public OrderStatus OrderStatus { get; set; }
			public OrderPaymentStatus? OrderPaymentStatus { get; set; }
			public decimal OrderSum { get; set; }
			public decimal OrderAllocation { get; set; }
			public bool IsMissingFromDocument { get; set; }
		}
	}
}
