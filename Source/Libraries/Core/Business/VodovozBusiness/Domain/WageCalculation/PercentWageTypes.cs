using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.WageCalculation
{
	public enum PercentWageTypes
	{
		[Display(Name = "За маршрутный лист")]
		RouteList,
		[Display(Name = "За сервисное обслуживание")]
		Service,
	}
}
