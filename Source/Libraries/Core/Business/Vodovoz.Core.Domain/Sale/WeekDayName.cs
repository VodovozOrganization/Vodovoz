using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

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
		[Appellative(
			Dative = "Понедельнику",
			DativePlural = "Понедельникам"
			)]
		[Display(Name = "Понедельник",
			ShortName = "ПН")]
		Monday = 1,
		/// <summary>
		/// Вторник
		/// </summary>
		[Appellative(
			Dative = "Вторнику",
			DativePlural = "Вторникам"
		)]
		[Display(Name = "Вторник",
			ShortName = "ВТ")]
		Tuesday = 2,
		/// <summary>
		/// Среда
		/// </summary>
		[Appellative(
			Dative = "Среде",
			DativePlural = "Средам"
		)]
		[Display(Name = "Среда",
			ShortName = "СР")]
		Wednesday = 3,
		/// <summary>
		/// Четверг
		/// </summary>
		[Appellative(
			Dative = "Четвергу",
			DativePlural = "Четвергам"
		)]
		[Display(Name = "Четверг",
			ShortName = "ЧТ")]
		Thursday = 4,
		/// <summary>
		/// Пятница
		/// </summary>
		[Appellative(
			Dative = "Пятнице",
			DativePlural = "Пятницам"
		)]
		[Display(Name = "Пятница",
			ShortName = "ПТ")]
		Friday = 5,
		/// <summary>
		/// Суббота
		/// </summary>
		[Appellative(
			Dative = "Субботе",
			DativePlural = "Субботам"
		)]
		[Display(Name = "Суббота",
			ShortName = "СБ")]
		Saturday = 6,
		/// <summary>
		/// Воскресенье
		/// </summary>
		[Appellative(
			Dative = "Воскресенью",
			DativePlural = "Воскресеньям"
		)]
		[Display(Name = "Воскресенье",
			ShortName = "ВС")]
		Sunday = 7
	}
}
