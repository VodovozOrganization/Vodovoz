using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	[Appellative(Nominative = "Тип контрагента",
		NominativePlural = "Типы контрагента")]
	public enum CounterpartyType
	{
		[Display(Name = "Покупатель")]
		Buyer,
		[Display(Name = "Поставщик")]
		Supplier,
		[Display(Name = "Дилер")]
		Dealer,
		[Display(Name = "Клиент РО")]
		AdvertisingDepartmentClient
	}
}
