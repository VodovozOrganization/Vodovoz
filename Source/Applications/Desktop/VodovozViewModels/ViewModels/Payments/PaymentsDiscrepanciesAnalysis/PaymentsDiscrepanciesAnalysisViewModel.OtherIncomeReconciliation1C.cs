using System;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Прочий приход, распознанный при чтении акта сверки 1С.
		/// </summary>
		public class OtherIncomeReconciliation1C
		{
			public string DocumentName { get; set; }
			public int? DocumentNumber { get; set; }
			public DateTime? DocumentDate { get; set; }
			public decimal IncomeSum { get; set; }
		}
	}
}
