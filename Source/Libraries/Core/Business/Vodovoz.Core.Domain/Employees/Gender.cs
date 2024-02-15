using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	public enum Gender
	{
		[Display(Name = "М")]
		male,
		[Display(Name = "Ж")]
		female
	}
}
