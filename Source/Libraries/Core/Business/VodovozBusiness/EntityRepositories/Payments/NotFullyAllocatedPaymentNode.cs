using System;

namespace Vodovoz.EntityRepositories.Payments
{
	public class NotFullyAllocatedPaymentNode
	{
		public int Id { get; set; }
		public decimal UnallocatedSum { get; set; }
		public DateTime PaymentDate { get; set; }
	}
}
