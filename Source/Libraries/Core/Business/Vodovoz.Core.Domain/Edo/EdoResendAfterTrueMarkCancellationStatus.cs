using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Состояние переотправки документа ЭДО после отмены вывода кодов из оборота в ЧЗ.
	/// </summary>
	public enum EdoResendAfterTrueMarkCancellationStatus
	{
		/// <summary>
		/// Ожидает отправки документа отмены в ЧЗ.
		/// </summary>
		[Display(Name = "Ожидает отправки отмены в ЧЗ")]
		WaitingForCancellation,

		/// <summary>
		/// Документ отмены отправлен в ЧЗ, ожидается результат обработки.
		/// </summary>
		[Display(Name = "Ожидает результата отмены в ЧЗ")]
		CancellationSent,

		/// <summary>
		/// ЧЗ не принял документ отмены или отправить его не удалось.
		/// </summary>
		[Display(Name = "Ошибка отмены в ЧЗ")]
		CancellationFailed,

		/// <summary>
		/// Отмена выполнена, заявка ЭДО готова к публикации.
		/// </summary>
		[Display(Name = "Готова к переотправке ЭДО")]
		ReadyToResend,

		/// <summary>
		/// Событие создания новой заявки ЭДО опубликовано.
		/// </summary>
		[Display(Name = "Переотправка ЭДО запущена")]
		Completed
	}
}
