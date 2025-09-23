using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Тип контрагента
	/// </summary>
	[Appellative(Nominative = "Тип контрагента",
		NominativePlural = "Типы контрагентов",
		GenitivePlural = "Типов контрагентов")]
	public enum CounterpartyType
	{
		/// <summary>
		/// Покупатель
		/// </summary>
		[Display(Name = "Покупатель")]
		Buyer,
		/// <summary>
		/// Поставщик
		/// </summary>
		[Display(Name = "Поставщик")]
		Supplier,
		/// <summary>
		/// Дилер
		/// </summary>
		[Display(Name = "Дилер")]
		Dealer,
		/// <summary>
		/// Клиент РО
		/// </summary>
		[Display(Name = "Клиент РО")]
		AdvertisingDepartmentClient
	}
}
