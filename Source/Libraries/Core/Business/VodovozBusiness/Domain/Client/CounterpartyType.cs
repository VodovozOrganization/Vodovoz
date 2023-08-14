using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
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
