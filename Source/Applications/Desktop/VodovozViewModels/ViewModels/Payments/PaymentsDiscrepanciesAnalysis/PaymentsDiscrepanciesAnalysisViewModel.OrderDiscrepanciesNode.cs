using System;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		public class OrderDiscrepanciesNode
		{
			public int OrderId { get; set; }
			public DateTime? OrderDeliveryDateInDatabase { get; set; }
			public DateTime? OrderDeliveryDateInDocument { get; set; }
			public OrderStatus? OrderStatus { get; set; }
			public OrderPaymentStatus? OrderPaymentStatus { get; set; }
			public decimal DocumentOrderSum { get; set; }
			public decimal ProgramOrderSum { get; set; }
			public decimal AllocatedSum { get; set; }
			public bool IsMissingFromDocument { get; set; }
			public string OrderClientNameInDatabase { get; set; }
			public string OrderClientInnInDatabase { get; set; }
			public bool OrderSumDiscrepancy => ProgramOrderSum != DocumentOrderSum;
			public DateTime? OrderDeliveryDate => OrderDeliveryDateInDatabase ?? OrderDeliveryDateInDocument;
		}
	}
}
