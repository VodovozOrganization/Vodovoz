using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Roboats
{
	public enum RoboatsEntityType
	{
		[Display(Name = "Графики доставки")]
		DeliverySchedules,
		[Display(Name = "Улицы")]
		Streets,
		[Display(Name = "Типы воды")]
		WaterTypes,
		[Display(Name = "Имена контрагентов")]
		CounterpartyName,
		[Display(Name = "Отчества контрагентов")]
		CounterpartyPatronymic
	}
}
