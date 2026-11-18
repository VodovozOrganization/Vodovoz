using System;
using Core.Infrastructure;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Receipts
{
	public class ReceiptCorrectionExplanatoryNoteJournalNode
	{
		public int RowNumber { get; set; }

		public int Id { get; set; }

		public int OrderId { get; set; }

		public int ProcessId { get; set; }

		public ReceiptCorrectionScenarioType ScenarioType { get; set; }

		public ReceiptCorrectionExplanatoryNoteTemplateType TemplateType { get; set; }

		public string SignerName { get; set; }

		public int? SignerSignatureId { get; set; }

		public string OrganizationName { get; set; }

		public string FiscalDocumentNumber { get; set; }

		public string Content { get; set; }

		public DateTime CreatedDate { get; set; }

		public string BasisTitle => ScenarioType.GetEnumDisplayName();
	}
}
