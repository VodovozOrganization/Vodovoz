using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	public enum EmployeeType
	{
		[Display(Name = "Сотрудник")]
		Employee,
		[Display(Name = "Стажер")]
		Trainee
	}
}
