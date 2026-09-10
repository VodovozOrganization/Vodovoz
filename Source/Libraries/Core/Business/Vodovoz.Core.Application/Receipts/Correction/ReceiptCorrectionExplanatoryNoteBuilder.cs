using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public class ReceiptCorrectionExplanatoryNoteBuilder : IReceiptCorrectionExplanatoryNoteBuilder
	{
		private const string SignerTitle = "Кассир";
		private const string StubSignerName = "Ф.И.О. кассира";
		private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

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
			var lines = new List<string>();

			lines.AddRange(BuildHeaderLines(context, resolvedSignerName));
			lines.Add(string.Empty);
			lines.Add("Объяснительная записка");
			lines.Add(string.Empty);
			lines.AddRange(BuildBodyLines(templateType, process, changeSet, context));
			lines.Add(string.Empty);
			lines.Add(DateTime.Now.ToString("dd.MM.yyyy", RuCulture));
			lines.Add("(факсимиле подписанта)");

			return new ReceiptCorrectionExplanatoryNote
			{
				Process = process,
				TemplateType = templateType,
				OrganizationId = organizationId ?? process.OrganizationId,
				SignerTitle = SignerTitle,
				SignerName = resolvedSignerName,
				SignerSignatureId = signerSignatureId,
				Content = string.Join("\n", lines),
				CreatedDate = DateTime.Now
			};
		}

		private static IEnumerable<string> BuildHeaderLines(ExplanatoryNoteBuildContext context, string signerName)
		{
			var orgName = context?.OrganizationName?.Trim();
			var leaderName = string.IsNullOrWhiteSpace(context?.LeaderFullName)
				? "Ф.И.О."
				: context.LeaderFullName.Trim();

			if(context?.IsIndividualEntrepreneur == true)
			{
				yield return "Индивидуальному предпринимателю";
				yield return leaderName;
			}
			else
			{
				yield return "Генеральному директору";
				if(!string.IsNullOrWhiteSpace(orgName))
				{
					yield return orgName;
				}
				yield return leaderName;
			}

			yield return "от кассира";
			yield return signerName;
		}

		private static IEnumerable<string> BuildBodyLines(
			ReceiptCorrectionExplanatoryNoteTemplateType templateType,
			ReceiptCorrectionProcess process,
			FiscalChangeSet changeSet,
			ExplanatoryNoteBuildContext context)
		{
			var fiscalNumber = process.BaselineFiscalDocumentNumber ?? "________";
			var fiscalDate = process.BaselineFiscalDocumentDate?.ToString("dd.MM.yyyy") ?? "________";
			var sum = process.BaselineSum.ToString("0.00", RuCulture);
			var orderId = context?.OrderId > 0 ? context.OrderId.ToString() : process.OrderId.ToString();

			switch(templateType)
			{
				case ReceiptCorrectionExplanatoryNoteTemplateType.FullCancellation:
					yield return "После доставки товара покупателю была оформлена продажа и пробит чек до окончательного подтверждения покупателем покупки и оплаты данной поставки. Покупатель от товара отказался в полном объеме.";
					yield return $"Прошу аннулировать чек № {fiscalNumber} от {fiscalDate} на сумму {sum}.";
					yield break;

				case ReceiptCorrectionExplanatoryNoteTemplateType.NomenclatureReplacement:
					yield return $"Я, {SignerTitle}, внес корректировки в чек № {fiscalNumber} на сумму {sum} в связи с тем, что после пробития чека покупатель захотел купить другой товар, имеющийся в наличии вместо пробитого в чеке.";
					yield return "Была произведена замена товара:";
					foreach(var line in BuildPositionsTableLines(changeSet))
					{
						yield return line;
					}
					yield break;

				case ReceiptCorrectionExplanatoryNoteTemplateType.QuantityOrAmountIncrease:
					yield return $"Я, {SignerTitle}, внес корректировки в чек № {fiscalNumber} на сумму {sum} в связи с тем, что после пробития чека покупатель захотел купить дополнительный товар имеющийся в наличии.";
					yield return "Мною был пробит корректирующий чек с увеличением количества проданного товара и суммы покупки:";
					foreach(var line in BuildPositionsTableLines(changeSet))
					{
						yield return line;
					}
					yield break;

				case ReceiptCorrectionExplanatoryNoteTemplateType.QuantityOrAmountDecrease:
					yield return $"Я, {SignerTitle}, внес корректировки в чек № {fiscalNumber} на сумму {sum} в связи с частичным отказом покупателя от покупки.";
					yield return "Мною был пробит корректирующий чек с уменьшением количества проданного товара и суммы покупки:";
					foreach(var line in BuildPositionsTableLines(changeSet))
					{
						yield return line;
					}
					yield break;

				case ReceiptCorrectionExplanatoryNoteTemplateType.TechnicalFailure:
				default:
					yield return $"В процессе оплаты от покупателя ({DateTime.Now:dd.MM.yyyy HH:mm}) из-за технического сбоя при продаже не был оформлен чек по заказу № {orderId}. После обнаружения ошибки, чек на сумму {sum} был оформлен и оплата продажи принята к учету.";
					yield break;
			}
		}

		private static IEnumerable<string> BuildPositionsTableLines(FiscalChangeSet changeSet)
		{
			yield return "№\tНаименование\tКоличество\tЦена\tСумма";

			var positions = changeSet?.PositionChanges?
				.Where(x => x != null)
				.ToList();

			if(positions == null || positions.Count == 0)
			{
				yield return "-\t-\t-\t-\t-";
				yield return "Итого\t\t\t\t-";
				yield break;
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

				yield return $"{index}\t{name}\t{FormatDecimal(quantity)}\t{FormatDecimal(price)}\t{FormatDecimal(lineSum)}";
				index++;
			}

			yield return $"Итого\t\t\t\t{FormatDecimal(total)}";
		}

		private static string FormatDecimal(decimal value) =>
			value.ToString("0.##", RuCulture);
	}
}
