using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	public enum EmployeeCategory
	{
		[Display(Name = "Офисный работник")]
		office,
		[Display(Name = "Водитель")]
		driver,
		[Display(Name = "Экспедитор")]
		forwarder
	}
}
