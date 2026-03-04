using NLog;
using NLog.Filters;

namespace VodovozHealthCheck.Logging
{
	/// <summary>
	/// Кастомный фильтр NLog, который подавляет (игнорирует) все сообщения логирования 
	/// при выполнении health-check запросов.
	/// </summary>
	/// <remarks>
	/// Фильтр применяется ко всем правилам логирования в конфигурации NLog.
	/// Решение о подавлении принимается на основе значения флага <see cref="LoggingContext.SuppressLogging"/>,
	/// который устанавливается в middleware <see cref="HealthCheckLoggingMiddleware"/> при обнаружении 
	/// health-check запроса (по заголовку X-Health-Check: true или по пути /health).
	/// 
	/// Возвращает:
	/// - <see cref="FilterResult.Ignore"/> — если логирование подавлено (health-check запрос)
	/// - <see cref="FilterResult.Log"/>   — если логирование разрешено (обычный запрос)
	/// 
	/// Для использования в конфигурации служб добавить в раздел <c>rules</c> фильтры <c>filters</c> с указанием типа фильтра
	/// <c>HealthCheckSuppress</c> и желаемого действия (обычно <c>Ignore</c>) в каждом правиле логирования,
	/// для которого нужно подавлять логи health-check запросов.
	/// 
	/// Пример в конфигурации NLog:
	/// 
	/// "NLog": {
	///  "rules": [
	///  {
	///    "logger": "*",
	///    "minLevel": "Debug",
	///    "writeTo": "seq",
	///    "filters": [
	///      {
	///        "type": "HealthCheckSuppress",
	///        "action": "Ignore"
	///	     }
	///    ]
	///  }
	///]
	/// </remarks>

	[Filter(LoggingConstants.HealthCheckSuppressLogFilterName)]
	internal class LoggingFilter : Filter
	{
		protected override FilterResult Check(LogEventInfo logEvent)
		{
			return LoggingContext.SuppressLogging ? FilterResult.Ignore : FilterResult.Log;
		}
	}
}
