using System;

namespace Vodovoz.EntityRepositories.Orders
{
	public class NotFullyPaidOrderNode
	{
		public int Id { get; set; }
		public DateTime? OrderDeliveryDate { get; set; }
		public DateTime? OrderCreationDate { get; set; }
		public decimal OrderSum { get; set; }
		public decimal AllocatedSum { get; set; }
	}
}
