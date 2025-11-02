using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	/// <summary>
	/// Статус сотрудника
	/// </summary>
	/// 
	[Appellative(
		Nominative = "Статус сотрудника",
		NominativePlural = "Статусы сотрудников")]	
	public enum EmployeeStatus
	{
		/// <summary>
		/// Работает
		/// </summary>
		[Display(Name = "Работает")]
		IsWorking,

		/// <summary>
		/// На расчете
		/// </summary>
		[Display(Name = "На расчете")]
		OnCalculation,

		/// <summary>
		/// В декрете
		/// </summary>
		[Display(Name = "В декрете")]
		OnMaternityLeave,

		/// <summary>
		/// Уволен
		/// </summary>
		[Display(Name = "Уволен")]
		IsFired
	}
}
