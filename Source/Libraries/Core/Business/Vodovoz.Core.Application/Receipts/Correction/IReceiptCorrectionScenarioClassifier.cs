using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public interface IReceiptCorrectionScenarioClassifier
	{
		ReceiptCorrectionScenarioType Classify(FiscalChangeSet changeSet);

		ReceiptCorrectionExplanatoryNoteTemplateType GetExplanatoryNoteTemplate(ReceiptCorrectionScenarioType scenarioType);

		IList<FiscalDocumentType> GetPlannedDocumentTypes(ReceiptCorrectionScenarioType scenarioType);
	}
}
