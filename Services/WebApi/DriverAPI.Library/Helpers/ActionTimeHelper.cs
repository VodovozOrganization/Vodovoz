using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace DriverAPI.Library.Helpers
{
	public class ActionTimeHelper : IActionTimeHelper
	{
		private readonly int _timeout;
		private readonly int _futureTimeout;
		private readonly ILogger<ActionTimeHelper> _logger;

		public ActionTimeHelper(IConfiguration configuration, ILogger<ActionTimeHelper> logger)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_timeout = configuration.GetValue<int>("PostActionTimeTimeOut");
			_futureTimeout = configuration.GetValue<int>("FutureAtionTimeTimeOut");
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public void Validate(DateTime recievedTime, DateTime actionTime)
		{
			if(actionTime > recievedTime.AddMinutes(_futureTimeout))
			{
				_logger.LogError($"Пришел запрос из будущего {actionTime} в {recievedTime}");
				throw new InvalidTimeZoneException("Нельзя отправлять запросы из будущего! Проверьте настройки системного времени вашего телефона");
			}

			if(recievedTime > actionTime.AddMinutes(_timeout))
			{
				_logger.LogError($"Пришел запрос из дальнего прошлого {actionTime} в {recievedTime}");
				throw new InvalidOperationException("Таймаут запроса операции");
			}
		}
	}
}
