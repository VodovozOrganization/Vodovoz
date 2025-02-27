using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Источник заявки на отправку документов ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "источник заявки на отправку документов ЭДО",
		NominativePlural = "источники заявок на отправку документов ЭДО"
	)]
	public enum CustomerEdoRequestSource
	{
		/// <summary>
		/// Не указано
		/// </summary>
		[Display(Name = "Не указано")]
		None,

		/// <summary>
		/// Склад
		/// </summary>
		[Display(Name = "Склад")]
		Warehouse,

		/// <summary>
		/// Водитель
		/// </summary>
		[Display(Name = "Водитель")]
		Driver,

		/// <summary>
		/// Самовывоз
		/// </summary>
		[Display(Name = "Самовывоз")]
		Selfdelivery,

		/// <summary>
		/// Заказ без отгрузки
		/// </summary>
		[Display(Name = "Заказ без отгрузки")]
		OrderWithoutShipment,

		/// <summary>
		/// Ручной (принудительный) запуск
		/// </summary>
		[Display(Name = "Вручную")]
		Manual
	}
}
