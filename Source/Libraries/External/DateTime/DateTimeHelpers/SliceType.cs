using System.ComponentModel.DataAnnotations;

namespace DateTimeHelpers
{
	public enum DateTimeSliceType
	{
		[Display(Name = "День")]
		Day,
		[Display(Name = "Неделя")]
		Week,
		[Display(Name = "Месяц")]
		Month,
		[Display(Name = "Квартал")]
		Quarter,
		[Display(Name = "Год")]
		Year
	}
}
