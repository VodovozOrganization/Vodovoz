using System;

namespace Vodovoz.Settings.WebApi
{
	/// <summary>
	/// Настройки идемпотентности запросов Web API
	/// </summary>
	public interface IApiIdempotencySettings
	{
		/// <summary>
		/// DriverApi: срок хранения ответа идемпотентного запроса от момента сохранения
		/// </summary>
		TimeSpan DriverApiResponseLifetime { get; }

		/// <summary>
		/// DriverApi: срок жизни метки «в обработке», должен быть больше максимального времени выполнения метода
		/// </summary>
		TimeSpan DriverApiMarkerLifetime { get; }

		/// <summary>
		/// DriverApi: сколько повторный запрос ждёт результат первого, должно быть меньше таймаута HTTP-запроса в мобильном приложении
		/// </summary>
		TimeSpan DriverApiReplayWaitTimeout { get; }
	}
}
