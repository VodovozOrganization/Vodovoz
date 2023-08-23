using System.ComponentModel.DataAnnotations;

namespace Vodovoz.FilterViewModels
{
	public partial class ComplaintFilterViewModel
	{
		public enum DateFilterType
		{
			[Display(Name = "план. завершения")]
			PlannedCompletionDate,
			[Display(Name = "факт. завершения")]
			ActualCompletionDate,
			[Display(Name = "создания")]
			CreationDate
		}
	}
}
