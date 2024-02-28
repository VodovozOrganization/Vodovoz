using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	public enum EmployeeStatus
	{
		[Display(Name = "Работает")]
		IsWorking,
		[Display(Name = "На расчете")]
		OnCalculation,
		[Display(Name = "В декрете")]
		OnMaternityLeave,
		[Display(Name = "Уволен")]
		IsFired
	}
}
