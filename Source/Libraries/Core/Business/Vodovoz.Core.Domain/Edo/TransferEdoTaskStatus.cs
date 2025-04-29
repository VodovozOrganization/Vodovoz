using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum TransferEdoTaskStatus
	{
		[Display(Name = "Ожидает запросов")]
		WaitingRequests,
		[Display(Name = "Подготовка к отправке")]
		PreparingToSend,
		[Display(Name = "Готов к отправке")]
		ReadyToSend,
		[Display(Name = "В работе")]
		InProgress,
		[Display(Name = "Завершена")]
		Completed
	}
}
