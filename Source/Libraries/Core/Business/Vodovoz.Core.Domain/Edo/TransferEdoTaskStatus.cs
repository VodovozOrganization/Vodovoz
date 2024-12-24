using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum TransferEdoTaskStatus
	{
		[Display(Name = "Ожидает запросов")]
		WaitingRequests,
		[Display(Name = "В работе")]
		InProgress,
		[Display(Name = "Завершена")]
		Completed
	}
}
