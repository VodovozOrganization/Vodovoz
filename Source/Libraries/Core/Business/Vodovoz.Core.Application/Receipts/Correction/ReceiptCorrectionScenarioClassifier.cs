using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public class ReceiptCorrectionScenarioClassifier : IReceiptCorrectionScenarioClassifier
	{
		public ReceiptCorrectionScenarioType Classify(FiscalChangeSet changeSet)
		{
			if(changeSet == null || !changeSet.HasChanges)
			{
				return ReceiptCorrectionScenarioType.None;
			}

			if(changeSet.IsFullCancellation)
			{
				return ReceiptCorrectionScenarioType.FullCancellation;
			}

			// Смена юрлица — обнуление на старой кассе + новый SALE на новой.
			if(changeSet.HasOrganizationChange)
			{
				return ReceiptCorrectionScenarioType.OrganizationChange;
			}

			if(changeSet.HasClientChange)
			{
				return ReceiptCorrectionScenarioType.ClientChange;
			}

			// Смена формы оплаты в рамках той же организации — SALE_CORRECTION.
			if(changeSet.HasPaymentTypeChange)
			{
				return ReceiptCorrectionScenarioType.PaymentTypeChange;
			}

			// Смена договора без смены орг — как правило тоже коррекция реквизитов чека.
			if(changeSet.HasContractChange)
			{
				return ReceiptCorrectionScenarioType.PaymentTypeChange;
			}

			if(changeSet.HasNomenclatureChange || changeSet.HasPieceItemPriceChange)
			{
				var hasItemsToReturn = changeSet.HasPieceItemPriceChange
					|| changeSet.PositionChanges.Any(x => x.NewQuantity < x.OldQuantity);

				if(hasItemsToReturn)
				{
					return ReceiptCorrectionScenarioType.NomenclatureChange;
				}
			}

			if(changeSet.HasQuantityOrAmountDecrease)
			{
				return ReceiptCorrectionScenarioType.QuantityOrAmountDecrease;
			}

			if(changeSet.HasQuantityOrAmountIncrease || changeSet.HasNomenclatureChange)
			{
				return ReceiptCorrectionScenarioType.QuantityOrAmountIncrease;
			}

			if(changeSet.HasDeliveryDateChange)
			{
				return ReceiptCorrectionScenarioType.DeliveryDateChange;
			}

			return ReceiptCorrectionScenarioType.TechnicalFailure;
		}

		public ReceiptCorrectionExplanatoryNoteTemplateType GetExplanatoryNoteTemplate(ReceiptCorrectionScenarioType scenarioType)
		{
			switch(scenarioType)
			{
				case ReceiptCorrectionScenarioType.FullCancellation:
					return ReceiptCorrectionExplanatoryNoteTemplateType.FullCancellation;
				case ReceiptCorrectionScenarioType.NomenclatureChange:
					return ReceiptCorrectionExplanatoryNoteTemplateType.NomenclatureReplacement;
				case ReceiptCorrectionScenarioType.QuantityOrAmountDecrease:
					return ReceiptCorrectionExplanatoryNoteTemplateType.QuantityOrAmountDecrease;
				case ReceiptCorrectionScenarioType.QuantityOrAmountIncrease:
					return ReceiptCorrectionExplanatoryNoteTemplateType.QuantityOrAmountIncrease;
				case ReceiptCorrectionScenarioType.OrganizationChange:
				case ReceiptCorrectionScenarioType.ClientChange:
				case ReceiptCorrectionScenarioType.PaymentTypeChange:
				case ReceiptCorrectionScenarioType.DeliveryDateChange:
				case ReceiptCorrectionScenarioType.TechnicalFailure:
				default:
					return ReceiptCorrectionExplanatoryNoteTemplateType.TechnicalFailure;
			}
		}

		public IList<FiscalDocumentType> GetPlannedDocumentTypes(ReceiptCorrectionScenarioType scenarioType)
		{
			switch(scenarioType)
			{
				case ReceiptCorrectionScenarioType.FullCancellation:
				case ReceiptCorrectionScenarioType.QuantityOrAmountDecrease:
					return new List<FiscalDocumentType> { FiscalDocumentType.Return };
				case ReceiptCorrectionScenarioType.OrganizationChange:
				case ReceiptCorrectionScenarioType.ClientChange:
				case ReceiptCorrectionScenarioType.NomenclatureChange:
					return new List<FiscalDocumentType>
					{
						FiscalDocumentType.Return,
						FiscalDocumentType.Sale
					};
				case ReceiptCorrectionScenarioType.PaymentTypeChange:
				case ReceiptCorrectionScenarioType.QuantityOrAmountIncrease:
				case ReceiptCorrectionScenarioType.DeliveryDateChange:
					return new List<FiscalDocumentType> { FiscalDocumentType.SaleCorrection };
				default:
					return new List<FiscalDocumentType>();
			}
		}
	}
}
