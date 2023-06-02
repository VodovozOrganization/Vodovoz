using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash.FinancialCategoriesGroups
{
	public enum GroupType
	{
		[Display(Name = "Группа статей")]
		Group,
		[Display(Name = "Статья")]
		Category
	}
}
