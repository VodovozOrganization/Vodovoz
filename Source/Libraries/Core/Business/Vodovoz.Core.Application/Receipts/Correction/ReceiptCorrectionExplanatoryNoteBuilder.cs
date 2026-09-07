using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public class ReceiptCorrectionExplanatoryNoteBuilder : IReceiptCorrectionExplanatoryNoteBuilder
	{
		private const string SignerTitle = "Кассир";
		private const string StubSignerName = "Ф.И.О. кассира";

		public ReceiptCorrectionExplanatoryNote Build(
			ReceiptCorrectionProcess process,
			ReceiptCorrectionExplanatoryNoteTemplateType templateType,
			FiscalChangeSet changeSet,
			string signerName = null,
			int? organizationId = null,
			int? signerSignatureId = null,
			ExplanatoryNoteBuildContext context = null)
		{
			if(process == null)
			{
				throw new ArgumentNullException(nameof(process));
			}

			var resolvedSignerName = string.IsNullOrWhiteSpace(signerName) ? StubSignerName : signerName.Trim();
			var content = new StringBuilder();

			AppendHeader(content, context, resolvedSignerName);
			content.AppendLine();
			content.AppendLine("Объяснительная записка");
			content.AppendLine();
			AppendBody(content, templateType, process, changeSet, context);
			content.AppendLine();
			content.AppendLine(DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU")));
			content.AppendLine("(факсимиле подписанта)");

			return new ReceiptCorrectionExplanatoryNote
			{
				Process = process,
				TemplateType = templateType,
				OrganizationId = organizationId ?? process.OrganizationId,
				SignerTitle = SignerTitle,
				SignerName = resolvedSignerName,
				SignerSignatureId = signerSignatureId,
				Content = content.ToString(),
				CreatedDate = DateTime.Now
			};
		}

		private static void AppendHeader(StringBuilder content, ExplanatoryNoteBuildContext context, string signerName)
		{
			var orgName = context?.OrganizationName?.Trim();
			var leaderName = string.IsNullOrWhiteSpace(context?.LeaderFullName)
				? "Ф.И.О."
				: context.LeaderFullName.Trim();

			if(context?.IsIndividualEntrepreneur == true)
			{
				content.AppendLine("Индивидуальному предпринимателю");
				content.AppendLine(leaderName);
			}
			else
			{
				content.AppendLine("Генеральному директору");
				if(!string.IsNullOrWhiteSpace(orgName))
				{
					content.AppendLine(orgName);
				}
				content.AppendLine(leaderName);
			}

			content.AppendLine("от кассира");
			content.AppendLine(signerName);
		}

		private static void AppendBody(
			StringBuilder content,
			ReceiptCorrectionExplanatoryNoteTemplateType templateType,
			ReceiptCorrectionProcess process,
			FiscalChangeSet changeSet,
			ExplanatoryNoteBuildContext context)
		{
			var fiscalNumber = process.BaselineFiscalDocumentNumber ?? "________";
			var fiscalDate = process.BaselineFiscalDocumentDate?.ToString("dd.MM.yyyy") ?? "________";
			var sum = process.BaselineSum.ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
			var orderId = context?.OrderId > 0 ? context.OrderId.ToString() : process.OrderId.ToString();

			switch(templateType)
			{
				case ReceiptCorrectionExplanatoryNoteTemplateType.FullCancellation:
					content.AppendLine("После доставки товара покупателю была оформлена продажа и пробит чек до окончательного подтверждения покупателем покупки и оплаты данной поставки. Покупатель от товара отказался в полном объеме.");
					content.AppendLine($"Прошу аннулировать чек № {fiscalNumber} от {fiscalDate} на сумму {sum}.");
					break;

				case ReceiptCorrectionExplanatoryNoteTemplateType.NomenclatureReplacement:
					content.AppendLine($"Я, {SignerTitle}, внес корректировки в чек № {fiscalNumber} на сумму {sum} в связи с тем, что после пробития чека покупатель захотел купить другой товар, имеющийся в наличии вместо пробитого в чеке.");
					content.AppendLine("Была произведена замена товара:");
					AppendPositionsTable(content, changeSet);
					break;

				case ReceiptCorrectionExplanatoryNoteTemplateType.QuantityOrAmountIncrease:
					content.AppendLine($"Я, {SignerTitle}, внес корректировки в чек № {fiscalNumber} на сумму {sum} в связи с тем, что после пробития чека покупатель захотел купить дополнительный товар имеющийся в наличии.");
					content.AppendLine("Мною был пробит корректирующий чек с увеличением количества проданного товара и суммы покупки:");
					AppendPositionsTable(content, changeSet);
					break;

				case ReceiptCorrectionExplanatoryNoteTemplateType.QuantityOrAmountDecrease:
					content.AppendLine($"Я, {SignerTitle}, внес корректировки в чек № {fiscalNumber} на сумму {sum} в связи с частичным отказом покупателя от покупки.");
					content.AppendLine("Мною был пробит корректирующий чек с уменьшением количества проданного товара и суммы покупки:");
					AppendPositionsTable(content, changeSet);
					break;

				case ReceiptCorrectionExplanatoryNoteTemplateType.TechnicalFailure:
				default:
					content.AppendLine($"В процессе оплаты от покупателя ({DateTime.Now:dd.MM.yyyy HH:mm}) из-за технического сбоя при продаже не был оформлен чек по заказу № {orderId}. После обнаружения ошибки, чек на сумму {sum} был оформлен и оплата продажи принята к учету.");
					break;
			}
		}

		private static void AppendPositionsTable(StringBuilder content, FiscalChangeSet changeSet)
		{
			content.AppendLine("№\tНаименование\tКоличество\tЦена\tСумма");

			var positions = changeSet?.PositionChanges?
				.Where(x => x != null)
				.ToList();

			if(positions == null || positions.Count == 0)
			{
				content.AppendLine("-\t-\t-\t-\t-");
				content.AppendLine("Итого\t\t\t\t-");
				return;
			}

			var index = 1;
			decimal total = 0m;

			foreach(var position in positions)
			{
				var quantity = position.NewQuantity != 0 ? position.NewQuantity : position.OldQuantity;
				var price = position.NewPrice != 0 ? position.NewPrice : position.OldPrice;
				var lineSum = Math.Round(Math.Abs(quantity) * price, 2, MidpointRounding.AwayFromZero);
				total += lineSum;

				var name = string.IsNullOrWhiteSpace(position.Name)
					? (position.NomenclatureId?.ToString() ?? "-")
					: position.Name;

				content.AppendLine($"{index}\t{name}\t{FormatDecimal(quantity)}\t{FormatDecimal(price)}\t{FormatDecimal(lineSum)}");
				index++;
			}

			content.AppendLine($"Итого\t\t\t\t{FormatDecimal(total)}");
		}

		private static string FormatDecimal(decimal value) =>
			value.ToString("0.##", CultureInfo.GetCultureInfo("ru-RU"));
	}
}
