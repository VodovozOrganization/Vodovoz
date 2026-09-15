using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Receipts;
using VodovozBusiness.Services.Receipts;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public static class ReceiptCorrectionUserMessages
	{
		public static string BuildConfirmationMessage(ReceiptCorrectionPreview preview, int? orderId = null)
		{
			if(preview == null || !preview.WillStartProcess)
			{
				return null;
			}

			var orderPart = orderId.HasValue ? $" №{orderId.Value}" : string.Empty;

			return string.Join("\n",
				$"По заказу{orderPart} уже пробит кассовый чек.",
				"При сохранении изменений будет автоматически запущена корректировка чека:",
				GetScenarioDescription(preview.ScenarioType),
				$"Будут сформированы документы: {GetDocumentTypesDescription(preview.PlannedDocumentTypes)}.",
				string.Empty,
				"Продолжить сохранение?");
		}

		public static string BuildConfirmationMessageForOrders(IEnumerable<(int OrderId, ReceiptCorrectionPreview Preview)> previews)
		{
			var items = previews?
				.Where(x => x.Preview?.WillStartProcess == true)
				.ToList();

			if(items == null || !items.Any())
			{
				return null;
			}

			if(items.Count == 1)
			{
				return BuildConfirmationMessage(items[0].Preview, items[0].OrderId);
			}

			var lines = new List<string>
			{
				"По следующим заказам уже пробиты кассовые чеки.",
				"При сохранении будут автоматически запущены корректировки:"
			};

			foreach(var item in items)
			{
				lines.Add(string.Empty);
				lines.Add(
					$"Заказ №{item.OrderId}: {GetScenarioDescription(item.Preview.ScenarioType)}"
					+ $" — {GetDocumentTypesDescription(item.Preview.PlannedDocumentTypes)}.");
			}

			lines.Add(string.Empty);
			lines.Add("Продолжить сохранение?");

			return string.Join("\n", lines);
		}

		private static string GetScenarioDescription(ReceiptCorrectionScenarioType scenarioType)
		{
			switch(scenarioType)
			{
				case ReceiptCorrectionScenarioType.FullCancellation:
					return "полная отмена заказа";
				case ReceiptCorrectionScenarioType.OrganizationChange:
					return "смена организации";
				case ReceiptCorrectionScenarioType.PaymentTypeChange:
					return "смена формы оплаты (чек коррекции)";
				case ReceiptCorrectionScenarioType.ClientChange:
					return "смена клиента";
				case ReceiptCorrectionScenarioType.NomenclatureChange:
					return "замена номенклатуры / цены (возврат + новый приход)";
				case ReceiptCorrectionScenarioType.QuantityOrAmountDecrease:
					return "уменьшение количества (возврат прихода)";
				case ReceiptCorrectionScenarioType.QuantityOrAmountIncrease:
					return "увеличение количества или суммы";
				case ReceiptCorrectionScenarioType.DeliveryDateChange:
					return "изменение даты доставки";
				default:
					return "корректировка чека";
			}
		}

		private static string GetDocumentTypesDescription(IReadOnlyList<FiscalDocumentType> documentTypes)
		{
			if(documentTypes == null || !documentTypes.Any())
			{
				return "не определены";
			}

			return string.Join(", ", documentTypes.Select(GetDocumentTypeDescription));
		}

		private static string GetDocumentTypeDescription(FiscalDocumentType documentType)
		{
			switch(documentType)
			{
				case FiscalDocumentType.Return:
					return "возврат прихода";
				case FiscalDocumentType.SaleCorrection:
					return "чек коррекции прихода";
				case FiscalDocumentType.Sale:
					return "приход";
				default:
					return documentType.ToString();
			}
		}
	}
}
