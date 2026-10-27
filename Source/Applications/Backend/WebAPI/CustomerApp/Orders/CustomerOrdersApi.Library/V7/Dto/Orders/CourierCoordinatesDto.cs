using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V7.Dto.Orders
{
	/// <summary>
	/// Координаты курьера и клиента для отображения на карте
	/// </summary>
	public class CourierCoordinatesDto
	{
		/// <summary>
		/// Статус отслеживания курьера
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public CourierTrackingStatusTypeDto? TrackingStatus { get; set; }

		/// <summary>
		/// Время в секундах для повторного вызова запроса
		/// </summary>
		public int TimeForRefresh { get; set; } = 60;

		/// <summary>
		/// Координаты клиента
		/// </summary>
		public CoordinatesDto ClientCoordinates { get; set; }

		/// <summary>
		/// Координаты курьера
		/// </summary>
		public CoordinatesDto CourierCoordinate { get; set; }
	}
}
