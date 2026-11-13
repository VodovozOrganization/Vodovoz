using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients.Mindbox
{
	/// <summary>
	/// Статус отправки контрагента в Mindbox
	/// </summary>
	public enum MindboxCounterpartyOutboxStatus
	{
		/// <summary>
		/// Неизвестно (статус ещё не определён)
		/// </summary>
		[Display(Name = "Неизвестно")]
		Unknown = 0,

		/// <summary>
		/// В очереди
		/// </summary>
		[Display(Name = "В очереди")]
		Queued = 1,

		/// <summary>
		/// Отправлен
		/// </summary>
		[Display(Name = "Отправлен")]
		Sent = 2,

		/// <summary>
		/// Ошибка данных, повтор не поможет
		/// </summary>
		[Display(Name = "Ошибка данных")]
		DataError = 3,

		/// <summary>
		/// Ошибка, исчерпаны повторы или глобальная ошибка
		/// </summary>
		[Display(Name = "Ошибка")]
		Error = 4
	}
}
