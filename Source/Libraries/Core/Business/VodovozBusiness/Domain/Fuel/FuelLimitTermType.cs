using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Fuel
{
	public enum FuelLimitTermType
	{
		[Display(Name = "Все дни")]
		AllDays = 1,
		[Display(Name = "Рабочие дни")]
		WorkingDays = 2,
		[Display(Name = "Выходные дни")]
		DaysOff = 3,
	}
}
