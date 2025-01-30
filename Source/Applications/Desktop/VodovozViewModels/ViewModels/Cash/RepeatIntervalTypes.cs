using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	/// <summary>
	/// Тип интервала повторений
	/// </summary>
	public enum RepeatIntervalTypes
	{
		/// <summary>
		/// День
		/// </summary>
		[Display(Name = "День")]
		Day,
		/// <summary>
		/// Неделя
		/// </summary>
		[Display(Name = "Неделя")]
		Week,
		/// <summary>
		/// Месяц
		/// </summary>
		[Display(Name = "Месяц")]
		Month,
		/// <summary>
		/// Год
		/// </summary>
		[Display(Name = "Год")]
		Year,
		/// <summary>
		/// N дней
		/// </summary>
		[Display(Name = "N дней")]
		NDays
	}
}
