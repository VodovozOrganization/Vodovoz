using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	public enum FineTypes
	{
		[Display(Name = "Стандартный")]
		Standart,
		[Display(Name = "Перерасход топлива")]
		FuelOverspending
	}
}

