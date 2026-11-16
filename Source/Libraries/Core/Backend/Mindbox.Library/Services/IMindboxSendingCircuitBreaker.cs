using System;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Приостанавливает отправку всей очереди при недоступности Mindbox или глобальной ошибке
	/// </summary>
	public interface IMindboxSendingCircuitBreaker
	{
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
		/// Проверяет, приостановлена ли отправка очереди
		/// </summary>
		/// <param name="now">Текущее время</param>
		/// <returns><c>true</c>, если отправка приостановлена; иначе <c>false</c></returns>
		bool IsPaused(DateTime now);

		/// <summary>
		/// Возвращает <c>true</c> один раз, когда Mindbox недоступен дольше указанного времени
		/// </summary>
		/// <param name="now">Текущее время</param>
		/// <param name="duration">Порог недоступности, после которого нужно отправить алерт</param>
		/// <returns><c>true</c>, если алерт о недоступности нужно отправить сейчас; иначе <c>false</c></returns>
		bool TryMarkUnavailableAlertSent(DateTime now, TimeSpan duration);

		/// <summary>
		/// Ставит очередь на паузу
		/// </summary>
		/// <param name="now">Текущее время</param>
		/// <param name="duration">Длительность паузы</param>
		/// <param name="reason">Причина паузы</param>
		void Pause(DateTime now, TimeSpan duration, string reason);

		/// <summary>
		/// Учитывает временную ошибку; после нескольких ошибок подряд очередь ставится на паузу
		/// </summary>
		/// <param name="now">Текущее время</param>
		/// <param name="reason">Причина ошибки</param>
		void RegisterTransientFailure(DateTime now, string reason);

		/// <summary>
		/// Сбрасывает паузу и счётчик временных ошибок
		/// </summary>
		void Reset();
	}
}
