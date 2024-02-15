using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	public enum Gender
	{
		[Display(Name = "М")]
		male,
		[Display(Name = "Ж")]
		female
	}
}
