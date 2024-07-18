using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Counterparties
{
	public partial class CallTaskFilterViewModel
	{
		public enum TaskFilterDateType
		{
			[Display(Name = "Дата создания задачи")]
			CreationTime,
			[Display(Name = "Дата выполнения задачи")]
			CompleteTaskDate,
			[Display(Name = "Период выполнения задачи")]
			DeadlinePeriod
		}
	}
}
