using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum ContractType
	{
		[Display(Name = "Безналичная")]
		Cashless,
		[Display(Name = "Наличная ФЛ")]
		CashFL,
		[Display(Name = "Наличная ЮЛ")]
		CashUL,
		[Display(Name = "Мир Напитков Наличная ФЛ")]
		CashBeveragesFL,
		[Display(Name = "Мир Напитков Наличная ЮЛ")]
		CashBeveragesUL,
		[Display(Name = "Бартер")]
		Barter
	}
}
