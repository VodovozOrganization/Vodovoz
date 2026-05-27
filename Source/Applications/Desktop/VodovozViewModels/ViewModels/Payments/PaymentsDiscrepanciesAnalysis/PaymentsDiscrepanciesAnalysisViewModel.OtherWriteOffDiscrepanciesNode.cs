using System;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		public class OtherWriteOffDiscrepanciesNode
		{
			public string DocumentName { get; set; }
			public int? DocumentNumber { get; set; }
			public DateTime? DocumentDate { get; set; }
			public decimal DocumentWriteOffSum { get; set; }
			public int? PaymentWriteOffId { get; set; }
			public int? PaymentWriteOffNumber { get; set; }
			public DateTime? PaymentWriteOffDate { get; set; }
			public decimal ProgramWriteOffSum { get; set; }
			public string Reason { get; set; }
			public bool IsMatchedWithoutNumber { get; set; }
			public bool WriteOffDiscrepancy => DocumentWriteOffSum != ProgramWriteOffSum;
			public DateTime? WriteOffDate => PaymentWriteOffDate ?? DocumentDate;
		}
	}
}
