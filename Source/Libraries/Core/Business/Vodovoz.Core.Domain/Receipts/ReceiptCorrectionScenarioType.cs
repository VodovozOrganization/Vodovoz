using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Receipts
{
	public enum ReceiptCorrectionScenarioType
	{
		[Display(Name = "Не требуется")]
		None,

		[Display(Name = "Смена организации")]
		OrganizationChange,

		[Display(Name = "Смена клиента")]
		ClientChange,

		[Display(Name = "Полная отмена заказа")]
		FullCancellation,

		[Display(Name = "Замена номенклатуры")]
		NomenclatureChange,

		[Display(Name = "Уменьшение количества или суммы")]
		QuantityOrAmountDecrease,

		[Display(Name = "Увеличение количества или суммы")]
		QuantityOrAmountIncrease,

		[Display(Name = "Изменение даты доставки")]
		DeliveryDateChange,

		[Display(Name = "Технический сбой / прочее")]
		TechnicalFailure,

		[Display(Name = "Смена формы оплаты")]
		PaymentTypeChange
	}
}
