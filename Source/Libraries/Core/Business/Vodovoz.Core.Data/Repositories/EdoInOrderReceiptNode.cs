using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	public class EdoInOrderReceiptNode
	{
		public int FiscalDocumentId { get; set; }
		public int OrderEdoTaskId { get; set; }
		public Guid DocumentGuid { get; set; }
		public string DocumentNumber { get; set; }
		public FiscalDocumentType DocumentType { get; set; }
		public DateTime CreationTime { get; set; }
		public FiscalDocumentStatus DocumentStatus { get; set; }
		public int Index { get; set; }
		public string Contact { get; set; }
		public string FiscalNumber { get; set; }
		public string FiscalMark { get; set; }
		public string FiscalKktNumber { get; set; }
		public DateTime? FiscalTime { get; set; }
		public string Cashier { get; set; }
		public string ClientInn { get; set; }
		public string FailureMessage { get; set; }
		public decimal Sum { get; set; }
	}
}
