using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.Helpers
{
	// TODO: Переместить в фильтр
	/// <summary>
	/// Помошник проверки времени
	/// </summary>
	public class ActionTimeHelper : IActionTimeHelper
	{
		private readonly int _timeout;
		private readonly int _futureTimeout;
		private readonly ILogger<ActionTimeHelper> _logger;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="configuration"></param>
		/// <param name="logger"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ActionTimeHelper(IConfiguration configuration, ILogger<ActionTimeHelper> logger)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_timeout = configuration.GetValue<int>("PostActionTimeTimeOutMinutes");
			_futureTimeout = configuration.GetValue<int>("FutureActionTimeTimeOutMinutes");
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[Obsolete]
		public void ThrowIfNotValid(DateTime recievedTime, DateTime actionTime)
		{
			if(actionTime > recievedTime.AddMinutes(_futureTimeout))
			{
				_logger.LogError("Пришел запрос из будущего {ActionTime} в {RecievedTime}",
					actionTime,
					recievedTime);
				throw new InvalidTimeZoneException("Нельзя отправлять запросы из будущего! Проверьте настройки системного времени вашего телефона");
			}

			if(recievedTime > actionTime.AddMinutes(_timeout))
			{
				_logger.LogError("Пришел запрос из дальнего прошлого {ActionTime} в {RecievedTime}",
					actionTime,
					recievedTime);
				throw new InvalidOperationException("Таймаут запроса операции");
			}
		}

		/// <summary>
		/// Проверка времени запроса и предоставленного времени операции
		/// </summary>
		/// <param name="recievedTime"></param>
		/// <param name="actionTime"></param>
		/// <returns></returns>
		public Result CheckRequestTime(DateTime recievedTime, DateTime actionTime)
		{
			if(actionTime > recievedTime.AddMinutes(_futureTimeout))
			{
				_logger.LogError("Пришел запрос из будущего {ActionTime} в {RecievedTime}",
					actionTime,
					recievedTime);

				return Result.Failure(Errors.DateTimeCheckErrors.TooEarly);
			}

			if(recievedTime > actionTime.AddMinutes(_timeout))
			{
				_logger.LogError("Пришел запрос из дальнего прошлого {ActionTime} в {RecievedTime}",
					actionTime,
					recievedTime);

				return Result.Failure(Errors.DateTimeCheckErrors.TooLate);
			}

			return Result.Success();
		}
	}
}
