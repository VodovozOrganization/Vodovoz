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
			/// <summary>
			/// Наименование документа 1С.
			/// </summary>
			public string DocumentName { get; set; }

			/// <summary>
			/// Номер документа 1С.
			/// </summary>
			public int? DocumentNumber { get; set; }

			/// <summary>
			/// Дата документа 1С.
			/// </summary>
			public DateTime? DocumentDate { get; set; }

			/// <summary>
			/// Сумма прочего прихода по акту сверки 1С.
			/// </summary>
			public decimal IncomeSum { get; set; }
		}
	}
}
