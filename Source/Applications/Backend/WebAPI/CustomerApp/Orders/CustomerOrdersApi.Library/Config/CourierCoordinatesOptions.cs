using System;

namespace CustomerOrdersApi.Library.Config
{
	/// <summary>
	/// Настройки передачи координат курьера для отображения на карте
	/// </summary>
	public class CourierCoordinatesOptions
	{
		/// <summary>
		/// Таймаут для определения потери обновления координат курьера
		/// </summary>
		public TimeSpan TrackingLostTimeout { get; set; }

		/// <summary>
		/// Время в секундах для повторного вызова запроса координат курьера в мобильном приложении
		/// </summary>
		public int TimeForRefreshInMobileApp { get; set; }
	}
}
