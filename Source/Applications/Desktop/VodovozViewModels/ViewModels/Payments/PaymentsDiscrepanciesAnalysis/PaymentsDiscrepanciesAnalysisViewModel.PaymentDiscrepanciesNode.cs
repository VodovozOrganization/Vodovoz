using System;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		public class PaymentDiscrepanciesNode
		{
			public int PaymentNum { get; set; }
			public DateTime PaymentDate { get; set; }
			public decimal DocumentPaymentSum { get; set; }
			public decimal ProgramPaymentSum { get; set; }
			public string PayerName { get; set; }
			public int CounterpartyId { get; set; }
			public string CounterpartyName { get; set; }
			public string CounterpartyInn { get; set; }
			public bool IsManuallyCreated { get; set; }
			public string PaymentPurpose { get; set; }
		}
	}
}
