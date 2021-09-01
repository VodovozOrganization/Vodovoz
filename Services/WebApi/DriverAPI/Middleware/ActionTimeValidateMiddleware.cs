using DriverAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DriverAPI.Middleware
{
	public class ActionTimeValidateMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ActionTimeValidateMiddleware> _logger;
		private readonly int _timeout;
		private readonly int _futureTimeout;

		public ActionTimeValidateMiddleware(
			RequestDelegate next,
			ILogger<ActionTimeValidateMiddleware> logger,
			IConfiguration configuration
			)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_timeout = configuration.GetValue<int>("PostActionTimeTimeOut");
			_futureTimeout = configuration.GetValue<int>("FutureAtionTimeTimeOut");
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			IDelayedAction requestDto = null;

			try
			{
				context.Request.EnableBuffering();
				requestDto = await context.Request.ReadFromJsonAsync<DelayedAction>();
			}
			catch(Exception){}
			finally
			{
				context.Request.Body.Position = 0;
			}

			if(requestDto != null)
			{
				var recievedTime = DateTime.Now;

				if(requestDto.ActionTime > recievedTime.AddMinutes(_futureTimeout))
				{
					_logger.LogError($"Пришел запрос из будущего {requestDto.ActionTime} в {recievedTime}");
					throw new InvalidTimeZoneException("Нельзя отправлять запросы из будущего! Проверьте настройки системного времени вашего телефона");
				}

				if(recievedTime > requestDto.ActionTime.AddMinutes(_timeout))
				{
					_logger.LogError($"Пришел запрос из дальнего прошлого {requestDto.ActionTime} в {recievedTime}");
					throw new InvalidOperationException("Таймаут запроса операции");
				}
			}

			await _next(context);
		}
	}
}
