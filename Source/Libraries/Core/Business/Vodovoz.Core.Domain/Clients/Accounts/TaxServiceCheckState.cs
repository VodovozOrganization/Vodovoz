using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients.Accounts
{
	/// <summary>
	/// Состояние проверки в ФНС
	/// </summary>
	public enum TaxServiceCheckState
	{
		/// <summary>
		/// В процессе
		/// </summary>
		[Display(Name = "В процессе")]
		InProgress,
		/// <summary>
		/// Готово
		/// </summary>
		[Display(Name = "Готово")]
		Done,
		/// <summary>
		/// Ошибка
		/// </summary>
		[Display(Name = "Ошибка")]
		Error,
		/// <summary>
		/// Ликвидирована
		/// </summary>
		[Display(Name = "Юр лицо ликвидировано")]
		IsLiquidated
	}
}
