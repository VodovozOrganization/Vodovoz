using System.Threading;

namespace VodovozHealthCheck.Logging
{
	/// <summary>
	/// Хранение и передача флага подавления логирования 
	/// в рамках текущего асинхронного потока выполнения (execution context).
	/// А также хранение идентификатора текущего запуска health-check, который используется для корреляции связанных логов в Seq.
	/// </summary>
	/// <remarks>
	/// Используется для отключения логирования при обработке 
	/// health-check запросов (например, к эндпоинту /health или с заголовком X-Health-Check: true).	
	/// Флаг <see cref="SuppressLogging"/> сохраняется через все await и смену потоков благодаря <see cref="AsyncLocal{T}"/>.
	/// А также используется для передачи уникального идентификатора запуска health-check в <see cref="LoggingFilter"/> и <see cref="LoggingMiddleware"/>.
	/// </remarks>
	public static class LoggingContext
	{
		private static readonly AsyncLocal<bool> _suppressLogging = new();

		/// <summary>
		/// Флаг, указывающий, нужно ли подавлять (игнорировать) все сообщения логирования
		/// </summary>
		public static bool SuppressLogging
		{
			get => _suppressLogging.Value;
			set => _suppressLogging.Value = value;
		}

		private static readonly AsyncLocal<string> _healthCheckRunId = new();

		// Идентификатор текущего запуска health-check.
		// Общий для корневого /health-запроса и всех вложенных self-call'ов, которые проверка инициирует внутри себя
		// Используется для корреляции связанных логов в Seq (поиск по одному значению покажет весь флоу проверки целиком)
		public static string HealthCheckRunId
		{
			get => _healthCheckRunId.Value;
			set => _healthCheckRunId.Value = value;
		}
	}
}
