using CustomerOrdersApi.Library.V6.Dto.Orders;
using System;

namespace CustomerOrdersApi.Library.V6.Services
{
public partial interface ICourierTrackingService
	{
		public class DriverPositionData
		{
			/// <summary>
			/// Установлен ли маршрут для заказа
			/// </summary>
			public bool EstablishedRoute { get; set; }

			/// <summary>
			/// Координаты курьера
			/// </summary>
			public CoordinatesDto CourierCoordinate { get; set; }

			/// <summary>
			/// Время последнего обновления координат курьера
			/// </summary>
			public DateTime? CoordinatesLastUpdateTime { get; set; }
		}
	}
}
