using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client.Mindbox
{
	/// <summary>
	/// Статус отправки контрагента в Mindbox
	/// </summary>
	public enum MindboxCounterpartyOutboxStatus
	{
		/// <summary>
		/// В очереди
		/// </summary>
		[Display(Name = "В очереди")]
		Queued = 0,

		/// <summary>
		/// Отправлен
		/// </summary>
		[Display(Name = "Отправлен")]
		Sent = 1,

		/// <summary>
		/// Ошибка данных, повтор не поможет
		/// </summary>
		[Display(Name = "Ошибка данных")]
		DataError = 2,

		/// <summary>
		/// Ошибка, исчерпаны повторы или глобальная ошибка
		/// </summary>
		[Display(Name = "Ошибка")]
		Error = 3
	}
}
