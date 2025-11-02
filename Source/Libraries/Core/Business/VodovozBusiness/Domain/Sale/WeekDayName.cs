using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Sale
{
	public enum WeekDayName
	{
		[Display(Name = "Сегодня",
			ShortName = "ДД")]
		Today = 0,
		[Display(Name = "Понедельник",
			ShortName = "ПН")]
		Monday = 1,
		[Display(Name = "Вторник",
			ShortName = "ВТ")]
		Tuesday = 2,
		[Display(Name = "Среда",
			ShortName = "СР")]
		Wednesday = 3,
		[Display(Name = "Четверг",
			ShortName = "ЧТ")]
		Thursday = 4,
		[Display(Name = "Пятница",
			ShortName = "ПТ")]
		Friday = 5,
		[Display(Name = "Суббота",
			ShortName = "СБ")]
		Saturday = 6,
		[Display(Name = "Воскресенье",
			ShortName = "ВС")]
		Sunday = 7
	}
}
