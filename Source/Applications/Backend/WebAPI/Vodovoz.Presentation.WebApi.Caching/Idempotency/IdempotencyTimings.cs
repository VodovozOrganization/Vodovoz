using System;

namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Сроки хранения и ожидания идемпотентных запросов для конкретного API
	/// </summary>
	public sealed class IdempotencyTimings
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="responseLifetime">Срок хранения сохранённого ответа от момента сохранения</param>
		/// <param name="markerLifetime">Срок жизни метки «в обработке»</param>
		/// <param name="replayWaitTimeout">Сколько повторный запрос ждёт результат первого</param>
		public IdempotencyTimings(TimeSpan responseLifetime, TimeSpan markerLifetime, TimeSpan replayWaitTimeout)
		{
			if(responseLifetime <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(responseLifetime), responseLifetime, "Срок должен быть больше нуля");
			}

			if(markerLifetime <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(markerLifetime), markerLifetime, "Срок должен быть больше нуля");
			}

			if(replayWaitTimeout < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException(nameof(replayWaitTimeout), replayWaitTimeout, "Срок не может быть отрицательным");
			}

			ResponseLifetime = responseLifetime;
			MarkerLifetime = markerLifetime;
			ReplayWaitTimeout = replayWaitTimeout;
		}

		/// <summary>
		/// Срок хранения сохранённого ответа от момента сохранения
		/// </summary>
		public TimeSpan ResponseLifetime { get; }

		/// <summary>
		/// Срок жизни метки "в обработке". Должен быть больше максимального времени выполнения метода,
		/// иначе метка истечёт раньше, чем будет сохранён ответ, и повтор выполнит метод ещё раз
		/// </summary>
		public TimeSpan MarkerLifetime { get; }

		/// <summary>
		/// Сколько повторный запрос ждёт результат первого, прежде чем получить 409
		/// </summary>
		public TimeSpan ReplayWaitTimeout { get; }
	}
}
