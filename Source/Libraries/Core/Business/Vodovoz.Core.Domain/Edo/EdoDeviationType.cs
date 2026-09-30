using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Тип отклонения документооборота ЭДО от ожидаемого хода обработки
	/// Порядок объявления значений задает порядок валидации
	/// </summary>
	public enum EdoDeviationType
	{
		/// <summary>
		/// По заявке не создана задача ЭДО
		/// </summary>
		[Display(Name = "Задача не создана по заявке")]
		TaskNotCreated,

		/// <summary>
		/// Задача дольше допустимого висит в статусе "Новая": обработчик
		/// к ней так и не приступил, при этом никакой проблемы по ней не заведено
		/// </summary>
		[Display(Name = "Задача не взята в работу")]
		TaskNotStarted,

		/// <summary>
		/// Итерация трансфера создана, но перенос кодов не запущен
		/// </summary>
		[Display(Name = "Трансфер не запущен")]
		TransferNotStarted,

		/// <summary>
		/// Исходящий документ создан, но не ушел провайдеру ЭДО
		/// </summary>
		[Display(Name = "Документ не отправлен провайдеру")]
		DocumentNotSentToProvider,

		/// <summary>
		/// Документооборот создан у провайдера ЭДО, но ответа по нему нет
		/// </summary>
		[Display(Name = "Нет ответа от провайдера ЭДО")]
		ProviderNoResponse,

		/// <summary>
		/// Документ ушел клиенту, но клиент не завершает документооборот
		/// </summary>
		[Display(Name = "Документооборот не принят клиентом")]
		ClientNotAcceptedDocflow,

		/// <summary>
		/// Документооборот ожидает аннулирования дольше допустимого
		/// </summary>
		[Display(Name = "Аннулирование не завершено")]
		CancellationNotCompleted,

		/// <summary>
		/// Документооборот завершен, но результат обработки кодов в ГИС МТ не получен
		/// </summary>
		[Display(Name = "Нет результата обработки кодов в ГИС МТ")]
		GisMtResultMissing,

		/// <summary>
		/// ГИС МТ не приняла коды по документообороту
		/// </summary>
		[Display(Name = "Коды не приняты в ГИС МТ")]
		GisMtRejected,

		/// <summary>
		/// Чек отправлен в кассу, но не фискализирован
		/// </summary>
		[Display(Name = "Чек не фискализирован")]
		ReceiptNotFiscalized,

		/// <summary>
		/// Задача не завершена дольше допустимого, но ни одно частное условие не сработало
		/// </summary>
		[Display(Name = "Задача зависла")]
		TaskStalled,

		/// <summary>
		/// Задача трансфера ждет заявок дольше допустимого
		/// </summary>
		[Display(Name = "Трансфер: ожидание заявок затянулось")]
		TransferWaitingRequestsTooLong,

		/// <summary>
		/// Задача трансфера собрана к отправке, но документ на перенос кодов так и не создан
		/// </summary>
		[Display(Name = "Трансфер: документ не создан")]
		TransferDocumentNotCreated,

		/// <summary>
		/// Документ трансфера создан, но документооборот у провайдера ЭДО не заведен
		/// </summary>
		[Display(Name = "Трансфер: документ не отправлен провайдеру")]
		TransferDocumentNotSentToProvider,

		/// <summary>
		/// Документооборот трансфера заведен у провайдера ЭДО, но ответа по нему нет
		/// </summary>
		[Display(Name = "Трансфер: нет ответа от провайдера ЭДО")]
		TransferProviderNoResponse,

		/// <summary>
		/// Документооборот трансфера завершен, но результат обработки кодов в ГИС МТ не получен
		/// </summary>
		[Display(Name = "Трансфер: нет результата обработки кодов в ГИС МТ")]
		TransferGisMtResultMissing,

		/// <summary>
		/// ГИС МТ не приняла коды по документообороту трансфера
		/// </summary>
		[Display(Name = "Трансфер: коды не приняты в ГИС МТ")]
		TransferGisMtRejected,

		/// <summary>
		/// Документооборот трансфера завершен, но коды так и не сменили владельца в ГИС МТ
		/// </summary>
		[Display(Name = "Трансфер: коды не сменили владельца в ГИС МТ")]
		TransferCodesNotMoved,

		/// <summary>
		/// Перенос кодов запущен, но не завершается
		/// </summary>
		[Display(Name = "Трансфер выполняется слишком долго")]
		TransferTooLong,

		/// <summary>
		/// Задача трансфера не завершена дольше допустимого, но ни одно частное условие не сработало
		/// </summary>
		[Display(Name = "Трансфер завис")]
		TransferStalled
	}
}
