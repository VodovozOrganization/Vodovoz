using System;

namespace DriverApi.Contracts.V5.Requests
{
	/// <summary>
	/// Запрос на подтверждение получения переноса
	/// </summary>
	public class ConfirmRouteListAddressTransferRecievedRequest
	{
		/// <summary>
		/// Идентификатор адреса маршрутного листа
		/// </summary>
		public int RouteListAddress { get; set; }

		/// <summary>
		/// Время нажатия на кнопку подтверждения в мобильном приложении водителей
		/// </summary>
		public DateTime ActionTimeUtc { get; set; }
	}
}
