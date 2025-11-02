using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Fuel
{
	public enum FuelLimitPeriodUnit
	{
		[Display(Name = "Разовый")]
		OneTime = 2,
		[Display(Name = "Сутки")]
		Day = 3,
		[Display(Name = "Неделя")]
		Week = 4,
		[Display(Name = "Месяц")]
		Month = 5,
		[Display(Name = "Квартал")]
		Quarter = 6,
		[Display(Name = "Год")]
		Year = 7
	}
}
