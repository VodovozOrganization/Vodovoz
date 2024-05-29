using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Fuel
{
	public enum FuelLimitUnit
	{
		[Display(Name = "Литр")]
		Liter,
		[Display(Name = "Рубль")]
		Ruble
	}
}
