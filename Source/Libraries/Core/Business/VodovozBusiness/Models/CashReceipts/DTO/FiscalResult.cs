using System;

namespace Vodovoz.Models.CashReceipts.DTO
{
	public class FiscalizationResult
	{
		public SendStatus SendStatus { get; set; }
		public long? FiscalDocumentNumber { get; set; }
		public DateTime? FiscalDocumentDate { get; set; }
		public FiscalDocumentStatus? Status { get; set; }
		public DateTime? StatusChangedTime { get; set; }
		public string FailDescription { get; set; }
	}
}
