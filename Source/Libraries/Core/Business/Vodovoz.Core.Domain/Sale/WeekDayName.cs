using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Sale
{
	/// <summary>
	/// Дни недели
	/// </summary>
	public enum WeekDayName
	{
		/// <summary>
		/// Сегодня
		/// </summary>
		[Display(Name = "Сегодня",
			ShortName = "ДД")]
		Today = 0,
		/// <summary>
		/// Понедельник
		/// </summary>
		[Display(Name = "Понедельник",
			ShortName = "ПН")]
		Monday = 1,
		/// <summary>
		/// Вторник
		/// </summary>
		[Display(Name = "Вторник",
			ShortName = "ВТ")]
		Tuesday = 2,
		/// <summary>
		/// Среда
		/// </summary>
		[Display(Name = "Среда",
			ShortName = "СР")]
		Wednesday = 3,
		/// <summary>
		/// Четверг
		/// </summary>
		[Display(Name = "Четверг",
			ShortName = "ЧТ")]
		Thursday = 4,
		/// <summary>
		/// Пятница
		/// </summary>
		[Display(Name = "Пятница",
			ShortName = "ПТ")]
		Friday = 5,
		/// <summary>
		/// Суббота
		/// </summary>
		[Display(Name = "Суббота",
			ShortName = "СБ")]
		Saturday = 6,
		/// <summary>
		/// Воскресенье
		/// </summary>
		[Display(Name = "Воскресенье",
			ShortName = "ВС")]
		Sunday = 7
	}
}
