using System;

namespace Vodovoz.Settings.Edo
{
	/// <summary>
	/// Настройки сервисов обработки проблем в ЭДО
	/// </summary>
	public interface IEdoProblemRoutineSettings
	{
		/// <summary>
		/// Таймаут обработки проблемы с оплатой при самовывозе
		/// </summary>
		TimeSpan SelfDeliveryPaidProblemTimeout { get; }

		/// <summary>
		/// Интервал работы воркера обработки проблем с оплатой при самовывозе
		/// </summary>
		TimeSpan SelfDeliveryPaidProblemWorkerInterval { get; }

		/// <summary>
		/// Таймаут обработки проблем с ошибкой отправки фискального документа
		/// </summary>
		TimeSpan FiscalDocumentSendErrorProblemTimeout { get; }

		/// <summary>
		/// Интервал работы воркера обработки проблем с ошибкой отправки фискального документа
		/// </summary>
		TimeSpan FiscalDocumentSendErrorProblemWorkerInterval { get; }
	}
}
