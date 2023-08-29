using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	public enum ExpenseType
	{
		[Display(Name = "Прочий расход")]
		Expense,
		[Display(Name = "Возврат по самовывозу")]
		ExpenseSelfDelivery,
		[Display(Name = "Аванс подотчетному лицу")]
		Advance,
		[Display(Name = "Аванс сотруднику")]
		EmployeeAdvance,
		[Display(Name = "Выдача зарплаты")]
		Salary
	}
}
