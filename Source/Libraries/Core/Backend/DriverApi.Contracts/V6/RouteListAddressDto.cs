using System;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Адрес маршрутного листа
	/// </summary>
	public class RouteListAddressDto
	{
		/// <summary>
		/// Номер адреса маршрутного листа
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Номер заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Статус адреса маршрутного листа
		/// </summary>
		public RouteListAddressDtoStatus Status { get; set; }

		/// <summary>
		/// Начало интервала доставки
		/// </summary>
		public DateTime DeliveryIntervalStart { get; set; }

		/// <summary>
		/// Конец интервала доставки
		/// </summary>
		public DateTime DeliveryIntervalEnd { get; set; }

		/// <summary>
		/// Адрес
		/// </summary>
		public AddressDto Address { get; set; }
	}
}
