using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	public enum FineCategory
	{
		[Display(Name = "Административный")]
		Administrative,
		[Display(Name = "ГИБДД")]
		GIBDD,
		[Display(Name = "Кассовая дисциплина")]
		CashDiscipline,
		[Display(Name = "Ущерб компании")]
		CompanyDamage
	}
}
