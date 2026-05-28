using System;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Строка прочего прихода из акта сверки 1С.
		/// </summary>
		public class OtherIncomeNode
		{
			public string DocumentName { get; set; }
			public int? DocumentNumber { get; set; }
			public DateTime? DocumentDate { get; set; }
			public decimal IncomeSum { get; set; }
		}
	}
}
