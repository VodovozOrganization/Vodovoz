using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using VodovozHealthCheck.Helpers;

namespace VodovozHealthCheck.Logging
{
	/// <summary>
	/// Управление флагом подавления логирования для health-check запросов.
	/// </summary>
	/// <remarks>
	/// Проверяет, является ли текущий HTTP-запрос проверкой работоспособности (health-check),
	/// и устанавливает флаг <see cref="LoggingContext.SuppressLogging"/>, если <c>true</c>, то
	/// все вызовы ILogger в рамках текущего асинхронного потока выполнения 
	/// подавляются кастомным фильтром NLog <see cref="LoggingFilter"/>.
	/// Должен быть зарегистрирован в пайплайне до <c>UseEndpoints</c>, чтобы охватывать все запросы.
	/// </remarks>

	internal class LoggingMiddleware
	{
		private readonly RequestDelegate _next;

		public LoggingMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			var isHealthCheck =
				HttpResponseHelper.IsHealthCheckRequest(context.Request)
				|| context.Request.Path.StartsWithSegments("/health")
				|| context.Request.Path.Value?.Equals("/health", StringComparison.OrdinalIgnoreCase) == true;

			if(isHealthCheck)
			{
				LoggingContext.SuppressLogging = true;
			}

			try
			{
				await _next(context);
			}
			finally
			{
				LoggingContext.SuppressLogging = false;
			}
		}
	}
}
