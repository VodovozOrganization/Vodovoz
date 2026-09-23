using System;

namespace Vodovoz.Settings.Edo
{
	public interface IEdoReceiptSettings
	{
		/// <summary>
		/// Адрес API сервиса ЭДО чеков
		/// </summary>
		string EdoReceiptApiUrl { get; }


		/// <summary>
		/// Id текущего регламентирующего документа для отраслевых реквизитов
		/// Необходим для разрешительного режима в чеках
		/// </summary>
		int IndustryRequisiteRegulatoryDocumentId { get; }


		/// <summary>
		/// Максимальное количество кодов ЧЗ в одном чеке
		/// </summary>
		int MaxCodesInReceiptCount { get; }

		/// <summary>
		/// Начало ночной паузы отправки чеков
		/// </summary>
		TimeSpan ReceiptSendPauseStartTime { get; }

		/// <summary>
		/// Окончание ночной паузы отправки чеков
		/// </summary>
		TimeSpan ReceiptSendPauseEndTime { get; }

		/// <summary>
		/// Интервал проверки чеков, зависших в очереди.
		/// </summary>
		TimeSpan ReceiptQueueNotificationWorkerInterval { get; }

		/// <summary>
		/// Интервал повторных уведомлений о чеке, зависшем в очереди.
		/// </summary>
		TimeSpan ReceiptQueueNotificationRepeatInterval { get; }

		/// <summary>
		/// Глубина выборки чеков по времени попадания в очередь, в календарных месяцах.
		/// </summary>
		int ReceiptQueueNotificationLookbackMonths { get; }
	}
}
