using System;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class ManualPaymentMatchingViewModelAllocatedNode
	{
		public int PaymentItemId { get; set; }
		public int OrderId { get; set; }
		public OrderStatus OrderStatus { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal OrderSum { get; set; }
		public decimal AllocatedSum { get; set; }
		public decimal AllAllocatedSum { get; set; }
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
		public AllocationStatus PaymentItemStatus { get; set; }
	}
}
