using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public interface IReceiptCorrectionExplanatoryNoteBuilder
	{
		ReceiptCorrectionExplanatoryNote Build(
			ReceiptCorrectionProcess process,
			ReceiptCorrectionExplanatoryNoteTemplateType templateType,
			FiscalChangeSet changeSet,
			string signerName = null,
			int? organizationId = null,
			int? signerSignatureId = null,
			ExplanatoryNoteBuildContext context = null);
	}
}
