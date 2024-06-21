using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum CallTaskStatus
	{
		[Display(Name = "Звонок")]
		Call,
		[Display(Name = "Задание")]
		Task,
		[Display(Name = "Сложный клиент")]
		DifficultClient,
		[Display(Name = "Первичка")]
		FirstClient,
		[Display(Name = "Cверка")]
		Reconciliation,
		[Display(Name = "Возврат залогов")]
		DepositReturn
	}
}
