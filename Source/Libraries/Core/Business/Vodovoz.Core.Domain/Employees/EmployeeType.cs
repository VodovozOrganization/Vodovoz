using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	public enum EmployeeType
	{
		[Display(Name = "Сотрудник")]
		Employee,
		[Display(Name = "Стажер")]
		Trainee
	}
}
