using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	[Appellative(
		Nominative = "Статус сотрудника",
		NominativePlural = "Статусы сотрудников")]
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
