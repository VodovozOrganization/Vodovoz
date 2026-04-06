using System.Collections.Generic;

namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Координаты курьера и клиента для отображения на карте
	/// </summary>
	public class CourierCoordinatesDto
	{
		/// <summary>
		/// Статус отслеживания курьера
		/// </summary>
		public CourierTrackingStatusTypeDto? TrackingStatus { get; set; }

		/// <summary>
		/// Время в секундах для повторного вызова запроса
		/// </summary>
		public int TimeForRefresh { get; set; } = 60;

		/// <summary>
		/// Координаты клиента
		/// </summary>
		public CoordinatesDto ClientCoordinates { get; private set; }

		/// <summary>
		/// Координаты курьера с момента выбора адреса водителем
		/// </summary>
		public IEnumerable<CoordinatesDto> CourierCoordinates { get; private set; }
	}
}
