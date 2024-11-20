using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	public enum EdoDocFlowStatus
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "В процессе")]
		InProgress,
		[Display(Name = "Документооборот завершен успешно")]
		Succeed,
		[Display(Name = "Предупреждение")]
		Warning,
		[Display(Name = "Ошибка")]
		Error,
		[Display(Name = "Не начат")]
		NotStarted,
		[Display(Name = "Завершен с различиями")]
		CompletedWithDivergences,
		[Display(Name = "Не принят")]
		NotAccepted,
		[Display(Name = "Подготовка к отправке")]
		PreparingToSend
	}
}
