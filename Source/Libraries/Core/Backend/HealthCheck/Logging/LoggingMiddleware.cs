using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Threading.Tasks;
using VodovozHealthCheck.Helpers;
using VodovozHealthCheck.Logging;

/// <summary>
/// Управление флагами логирования для health-check запросов.
/// </summary>
/// <remarks>
/// Проверяет, является ли текущий HTTP-запрос проверкой работоспособности (health-check),
/// и устанавливает флаг <see cref="LoggingContext.SuppressLogging"/>, если <c>true</c>, то
/// все вызовы ILogger в рамках текущего асинхронного потока выполнения 
/// подавляются кастомным фильтром NLog <see cref="LoggingFilter"/>.
/// Должен быть зарегистрирован в пайплайне до <c>UseEndpoints</c>, чтобы охватывать все запросы.
/// Также устанавливает уникальный идентификатор запуска health-check в <see cref="LoggingContext.HealthCheckRunId"/>
/// </remarks>
internal class LoggingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<LoggingMiddleware> _logger;

	public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task Invoke(HttpContext context)
	{
		var isHealthCheck =
			HttpResponseHelper.IsHealthCheckRequest(context.Request)
			|| context.Request.Path.StartsWithSegments("/health")
			|| context.Request.Path.Value?.Equals("/health", StringComparison.OrdinalIgnoreCase) == true;

		string runId = null;

		if(isHealthCheck)
		{
			context.Request.Headers.TryGetValue(HttpResponseHelper.HealthCheckHeaderName, out var incomingRunId);

			runId = !string.IsNullOrEmpty(incomingRunId)
				? incomingRunId.ToString()
				: Guid.NewGuid().ToString("N");			

			LoggingContext.HealthCheckRunId = runId;
			context.Items[HttpResponseHelper.HealthCheckRunIdItemsKey] = runId;

			LoggingContext.SuppressLogging = true;
		}

		using(ScopeContext.PushProperty("HealthCheckRunId", runId))
		{
			try
			{
				await _next(context);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Необработанное исключение при обработке {Method} {Path}. HealthCheckRunId={RunId}",
					context.Request.Method,
					context.Request.Path,
					runId);

				throw;
			}
			finally
			{
				LoggingContext.SuppressLogging = false;

				if(isHealthCheck)
				{
					LoggingContext.HealthCheckRunId = null;
				}
			}
		}
	}
}
