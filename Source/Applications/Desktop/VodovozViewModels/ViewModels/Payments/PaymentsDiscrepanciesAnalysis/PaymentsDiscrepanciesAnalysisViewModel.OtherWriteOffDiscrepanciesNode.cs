using System;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		/// <summary>
		/// Строка сверки прочего списания из акта 1С со списанием в ДВ.
		/// </summary>
		public class OtherWriteOffDiscrepanciesNode
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
			/// Сумма прочего списания по акту сверки 1С.
			/// </summary>
			public decimal DocumentWriteOffSum { get; set; }

			/// <summary>
			/// Идентификатор списания в ДВ.
			/// </summary>
			public int? PaymentWriteOffId { get; set; }

			/// <summary>
			/// Номер списания в ДВ.
			/// </summary>
			public int? PaymentWriteOffNumber { get; set; }

			/// <summary>
			/// Дата списания в ДВ.
			/// </summary>
			public DateTime? PaymentWriteOffDate { get; set; }

			/// <summary>
			/// Сумма списания в ДВ.
			/// </summary>
			public decimal ProgramWriteOffSum { get; set; }

			/// <summary>
			/// Причина списания в ДВ.
			/// </summary>
			public string Reason { get; set; }

			/// <summary>
			/// Признак того, что списание сопоставлено без учета номера документа.
			/// </summary>
			public bool IsMatchedWithoutNumber { get; set; }

			/// <summary>
			/// Признак расхождения суммы списания между актом 1С и ДВ.
			/// </summary>
			public bool WriteOffDiscrepancy => DocumentWriteOffSum != ProgramWriteOffSum;

			/// <summary>
			/// Дата списания для отображения в сверке.
			/// </summary>
			public DateTime? WriteOffDate => PaymentWriteOffDate ?? DocumentDate;
		}
	}
}
