using System;
using Vodovoz.Core.Domain;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// Данные с координатами курьера и адреса по онлайн заказу
	/// </summary>
	public class CourierCoordinates
	{
		/// <summary>
		/// Id онлайн заказа в ИПЗ
		/// </summary>
		public Guid ExternalOrderId { get; set; }
		/// <summary>
		/// Последние координаты курьера(водителя)
		/// </summary>
		public PointCoordinates CourierCoordinate { get; set; }
		/// <summary>
		/// Координаты адреса
		/// </summary>
		public PointCoordinates DeliveryPointCoordinate { get; set; }
	}
}
