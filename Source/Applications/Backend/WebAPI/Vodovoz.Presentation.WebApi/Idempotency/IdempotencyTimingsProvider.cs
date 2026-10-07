using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;

namespace Vodovoz.Presentation.WebApi.Idempotency
{
	/// <summary>
	/// Кэш сроков идемпотентности поверх настроек из БД.
	/// Без него отсутствующая строка настроек вызывала бы перечитывание всей таблицы настроек на каждом запросе,
	/// а на пути повтора оставалось бы обращение к основной БД. Сроки перечитываются не чаще раза в минуту на процесс
	/// </summary>
	internal sealed class IdempotencyTimingsProvider
	{
		private static readonly TimeSpan _timingsCacheDuration = TimeSpan.FromMinutes(1);

		private readonly ILogger<IdempotencyTimingsProvider> _logger;
		private readonly Func<IdempotencyTimings> _timingsFactory;
		private readonly object _refreshLock = new object();

		private volatile CachedTimings _cachedTimings;
		private IdempotencyTimings _lastSuccessfulTimings;

		public IdempotencyTimingsProvider(
			ILogger<IdempotencyTimingsProvider> logger,
			Func<IdempotencyTimings> timingsFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_timingsFactory = timingsFactory ?? throw new ArgumentNullException(nameof(timingsFactory));
		}

		/// <summary>
		/// Получение сроков идемпотентности
		/// </summary>
		/// <param name="timings">Сроки</param>
		/// <returns><see langword="false"/>, если сроки прочитать не удалось и удачно прочитанных ранее нет</returns>
		public bool TryGetTimings(out IdempotencyTimings timings)
		{
			var cachedTimings = _cachedTimings;

			if(cachedTimings is null || cachedTimings.ExpiresAtUtc <= DateTime.UtcNow)
			{
				lock(_refreshLock)
				{
					cachedTimings = _cachedTimings;

					if(cachedTimings is null || cachedTimings.ExpiresAtUtc <= DateTime.UtcNow)
					{
						cachedTimings = new CachedTimings(ReadTimings(), DateTime.UtcNow.Add(_timingsCacheDuration));
						_cachedTimings = cachedTimings;
					}
				}
			}

			timings = cachedTimings.Timings;
			return timings != null;
		}

		private IdempotencyTimings ReadTimings()
		{
			try
			{
				_lastSuccessfulTimings = _timingsFactory();
				return _lastSuccessfulTimings;
			}
			catch(Exception ex)
			{
				if(_lastSuccessfulTimings is null)
				{
					_logger.LogWarning(
						ex,
						"Не удалось прочитать настройки идемпотентности, запросы выполняются без идемпотентности. "
							+ "Повторная попытка через {RetryAfter}",
						_timingsCacheDuration);
				}
				else
				{
					_logger.LogWarning(
						ex,
						"Не удалось прочитать настройки идемпотентности, используются прочитанные ранее. Повторная попытка через {RetryAfter}",
						_timingsCacheDuration);
				}

				return _lastSuccessfulTimings;
			}
		}

		private sealed class CachedTimings
		{
			public CachedTimings(IdempotencyTimings timings, DateTime expiresAtUtc)
			{
				Timings = timings;
				ExpiresAtUtc = expiresAtUtc;
			}

			public IdempotencyTimings Timings { get; }

			public DateTime ExpiresAtUtc { get; }
		}
	}
}
