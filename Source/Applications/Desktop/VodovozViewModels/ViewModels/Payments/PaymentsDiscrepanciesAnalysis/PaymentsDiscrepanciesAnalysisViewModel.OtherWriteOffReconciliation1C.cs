using System;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Прочее списание, распознанное при чтении акта сверки 1С.
		/// </summary>
		public class OtherWriteOffReconciliation1C
		{
			public string DocumentName { get; set; }
			public int? DocumentNumber { get; set; }
			public DateTime? DocumentDate { get; set; }
			public decimal WriteOffSum { get; set; }
		}
	}
}
