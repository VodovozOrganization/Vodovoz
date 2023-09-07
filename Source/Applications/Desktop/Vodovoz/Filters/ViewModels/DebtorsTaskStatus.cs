using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Filters.ViewModels
{
	public enum DebtorsTaskStatus
	{
		[Display(Name = "Да")]
		HasTask,
		[Display(Name = "Нет")]
		WithoutTask
	}
}
