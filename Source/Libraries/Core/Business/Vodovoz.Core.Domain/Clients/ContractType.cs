using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Тип договора
	/// </summary>
	public enum ContractType
	{
		/// <summary>
		/// Безналичная
		/// </summary>
		[Display(Name = "Безналичная")]
		Cashless,
		/// <summary>
		/// Наличная ФЛ
		/// </summary>
		[Display(Name = "Наличная ФЛ")]
		CashFL,
		/// <summary>
		/// Наличная ЮЛ
		/// </summary>
		[Display(Name = "Наличная ЮЛ")]
		CashUL,
		/// <summary>
		/// Мир Напитков Наличная ФЛ
		/// </summary>
		[Display(Name = "Мир Напитков Наличная ФЛ")]
		CashBeveragesFL,
		/// <summary>
		/// Мир Напитков Наличная ЮЛ
		/// </summary>
		[Display(Name = "Мир Напитков Наличная ЮЛ")]
		CashBeveragesUL,
		/// <summary>
		/// Бартер
		/// </summary>
		[Display(Name = "Бартер")]
		Barter
	}
}
