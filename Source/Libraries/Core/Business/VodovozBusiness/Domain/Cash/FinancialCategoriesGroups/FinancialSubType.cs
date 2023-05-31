using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash.FinancialCategoriesGroups
{
	public enum FinancialSubType
	{
		[Display(Name = "Приход")]
		Income,
		[Display(Name = "Расход")]
		Expense
	}
}
