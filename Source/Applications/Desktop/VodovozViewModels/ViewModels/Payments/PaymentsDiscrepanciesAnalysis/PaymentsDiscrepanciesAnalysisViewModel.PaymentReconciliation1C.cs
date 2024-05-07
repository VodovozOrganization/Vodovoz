using System;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		public class PaymentReconciliation1C
		{
			public int PaymentNum { get; set; }
			public DateTime PaymentDate { get; set; }
			public decimal PaymentSum { get; set; }
		}
	}
}
