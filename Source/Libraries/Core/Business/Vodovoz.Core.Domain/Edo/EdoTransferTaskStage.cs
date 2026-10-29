using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum EdoTransferTaskStage
	{
		[Display(Name = "Ожидает запросов")]
		WaitingRequests,
		[Display(Name = "Подготовка")]
		PreparingToSend,
		[Display(Name = "Отправляется")]
		ReadyToSend,
		[Display(Name = "В работе")]
		InProgress,
		[Display(Name = "Завершен")]
		Completed
	}
}
