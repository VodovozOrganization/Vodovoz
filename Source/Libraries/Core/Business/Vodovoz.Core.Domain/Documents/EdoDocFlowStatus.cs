using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Статусы ЭДО
	/// </summary>
	public enum EdoDocFlowStatus
	{
		/// <summary>
		/// Неизвестно
		/// </summary>
		[Display(Name = "Неизвестно")]
		Unknown,
		/// <summary>
		/// В процессе
		/// </summary>
		[Display(Name = "В процессе")]
		InProgress,
		/// <summary>
		/// Документооборот завершен успешно
		/// </summary>
		[Display(Name = "Документооборот завершен успешно")]
		Succeed,
		/// <summary>
		/// Предупреждение
		/// </summary>
		[Display(Name = "Предупреждение")]
		Warning,
		/// <summary>
		/// Ошибка
		/// </summary>
		[Display(Name = "Ошибка")]
		Error,
		/// <summary>
		/// Не начат
		/// </summary>
		[Display(Name = "Не начат")]
		NotStarted,
		/// <summary>
		/// Завершен с уточнениями
		/// </summary>
		[Display(Name = "Завершен с уточнениями")]
		CompletedWithDivergences,
		/// <summary>
		/// Не принят
		/// </summary>
		[Display(Name = "Не принят")]
		NotAccepted,
		/// <summary>
		/// Ожидает аннулирования
		/// </summary>
		[Display(Name = "Ожидает аннулирования")]
		WaitingForCancellation,
		/// <summary>
		/// Аннулирован
		/// </summary>
		[Display(Name = "Аннулирован")]
		Cancelled,
		/// <summary>
		/// Подготовка к отправке
		/// </summary>
		[Display(Name = "Подготовка к отправке")]
		PreparingToSend
	}
}
