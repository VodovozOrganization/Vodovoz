namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		public class CounterpartyBalance1C
		{
			public string Inn { get; set; }
			public decimal? Debit { get; set; }
			public decimal? Credit { get; set; }
			public decimal Balance => (Credit ?? 0) - (Debit ?? 0);
		}
	}
}
