using System;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Приостанавливает отправку всей очереди при недоступности Mindbox или глобальной ошибке
	/// </summary>
	public interface IMindboxSendingCircuitBreaker
	{
		/// <summary>
		/// Признак паузы на текущий момент
		/// </summary>
		bool IsPaused(DateTime now);

		/// <summary>
		/// Дата окончания паузы
		/// </summary>
		DateTime? PausedUntil { get; }

		/// <summary>
		/// Причина паузы
		/// </summary>
		string PauseReason { get; }

		/// <summary>
		/// Начало периода недоступности Mindbox
		/// </summary>
		DateTime? UnavailableSince { get; }

		/// <summary>
		/// Возвращает true один раз, когда Mindbox недоступен дольше указанного времени
		/// </summary>
		bool TryMarkUnavailableAlertSent(DateTime now, TimeSpan duration);

		/// <summary>
		/// Поставить очередь на паузу
		/// </summary>
		void Pause(DateTime now, TimeSpan duration, string reason);

		/// <summary>
		/// Учесть временную ошибку; после нескольких подряд очередь ставится на паузу
		/// </summary>
		void RegisterTransientFailure(DateTime now, string reason);

		/// <summary>
		/// Сбросить паузу и счетчик временных ошибок
		/// </summary>
		void Reset();
	}
}
