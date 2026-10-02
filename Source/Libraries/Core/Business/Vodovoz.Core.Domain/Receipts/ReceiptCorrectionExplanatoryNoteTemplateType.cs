using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Receipts
{
	public enum ReceiptCorrectionExplanatoryNoteTemplateType
	{
		[Display(Name = "Технический сбой")]
		TechnicalFailure,

		[Display(Name = "Полный отказ покупателя")]
		FullCancellation,

		[Display(Name = "Замена товара")]
		NomenclatureReplacement,

		[Display(Name = "Увеличение количества или суммы")]
		QuantityOrAmountIncrease,

		[Display(Name = "Уменьшение количества или суммы")]
		QuantityOrAmountDecrease
	}
}
