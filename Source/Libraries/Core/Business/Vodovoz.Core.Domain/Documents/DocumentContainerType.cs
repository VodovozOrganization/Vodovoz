using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Тип контейнера документооборота
	/// WS - используется потому, что enum в NHibernate не может быть более 36 символов для 1 значения
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "тип контейнера документооборота",
		NominativePlural = "типы контейнеров документооборота",
		Genitive = "типе контейнера документооборота",
		GenitivePlural = "типах контейнеров документооборота",
		Accusative = "типа контейнера документооборота",
		AccusativePlural = "типов контейнеров документооборота",
		Prepositional = "типах контейнера документооборота",
		PrepositionalPlural = "типах контейнеров документооборота")]
	public enum DocumentContainerType
	{
		/// <summary>
		/// УПД
		/// </summary>
		[Display(Name = "УПД")]
		Upd,

		/// <summary>
		/// Счёт
		/// </summary>
		[Display(Name = "Счёт")]
		Bill,

		/// <summary>
		/// Счет без отгрузки на предоплату
		/// </summary>
		[Display(Name = "Счет без отгрузки на предоплату")]
		BillWSForAdvancePayment,

		/// <summary>
		/// Счет без отгрузки на долг
		/// </summary>
		[Display(Name = "Cчет без отгрузки на долг")]
		BillWSForDebt,

		/// <summary>
		/// Счет без отгрузки на постоплату
		/// </summary>
		[Display(Name = "Cчет без отгрузки на постоплату")]
		BillWSForPayment
	}
}
