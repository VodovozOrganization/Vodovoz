using System;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		public class OrderReconciliation1C
		{
			public int OrderId { get; set; }
			public DateTime? OrderDeliveryDate { get; set; }
			public decimal OrderSum { get; set; }

		}
	}
}
