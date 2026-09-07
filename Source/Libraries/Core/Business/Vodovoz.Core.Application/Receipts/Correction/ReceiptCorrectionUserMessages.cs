using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			var builder = new StringBuilder();
			builder.Append("По заказу");

			if(orderId.HasValue)
			{
				builder.Append($" №{orderId.Value}");
			}

			builder.Append(" уже пробит кассовый чек.");
			builder.AppendLine();
			builder.Append("При сохранении изменений будет автоматически запущена корректировка чека:");
			builder.AppendLine();
			builder.AppendLine(GetScenarioDescription(preview.ScenarioType));
			builder.Append("Будут сформированы документы: ");
			builder.Append(GetDocumentTypesDescription(preview.PlannedDocumentTypes));
			builder.AppendLine(".");
			builder.AppendLine();
			builder.Append("Продолжить сохранение?");

			return builder.ToString();
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

			var builder = new StringBuilder();
			builder.AppendLine("По следующим заказам уже пробиты кассовые чеки.");
			builder.AppendLine("При сохранении будут автоматически запущены корректировки:");

			foreach(var item in items)
			{
				builder.AppendLine();
				builder.Append($"Заказ №{item.OrderId}: ");
				builder.Append(GetScenarioDescription(item.Preview.ScenarioType));
				builder.Append(" — ");
				builder.Append(GetDocumentTypesDescription(item.Preview.PlannedDocumentTypes));
				builder.Append('.');
			}

			builder.AppendLine();
			builder.AppendLine();
			builder.Append("Продолжить сохранение?");

			return builder.ToString();
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
