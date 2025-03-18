using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Аггрегация возможных статусов документа от всех провайдеров ЭДО
	/// </summary>
	public enum EdoDocumentStatus
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
		[Display(Name = "Аннулирован")]
		Cancelled,
		[Display(Name = "Ожидает аннулирования")]
		WaitingForCancellation
	}
}
