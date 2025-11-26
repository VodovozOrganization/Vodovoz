using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Тип заявки на отправку документов ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "тип заявки на отправку документов ЭДО",
		NominativePlural = "типы заявок на отправку документов ЭДО"
	)]
	public enum CustomerEdoRequestType
	{
		/// <summary>
		/// Заказ
		/// </summary>
		[Display(Name = "Заказ")]
		Order,

		/// <summary>
		/// Счет без отгрузки на предоплату
		/// </summary>
		[Display(Name = "Счет без отгрузки на предоплату")]
		OrderWithoutShipmentForAdvancePayment,

		/// <summary>
		/// Счет без отгрузки на долг
		/// </summary>
		[Display(Name = "Счет без отгрузки на долг")]
		OrderWithoutShipmentForDebt,

		/// <summary>
		/// Счет без отгрузки на постоплату
		/// </summary>
		[Display(Name = "Счет без отгрузки на постоплату")]
		OrderWithoutShipmentForPayment,
	}
}
