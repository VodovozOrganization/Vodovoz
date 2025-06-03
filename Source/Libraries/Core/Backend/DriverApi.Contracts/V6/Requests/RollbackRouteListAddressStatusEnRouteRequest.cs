using System;
using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V6.Requests
{
	/// <summary>
	/// Запрос на возвращение маршрутного листа в статус В пути
	/// </summary>
	public class RollbackRouteListAddressStatusEnRouteRequest
	{
		/// <summary>
		/// Номер адреса маршрутного листа
		/// </summary>
		[Required]
		public int RoutelistAddressId { get; set; }

		/// <summary>
		/// Время операции на стороне мобильного приложения водителя
		/// </summary>
		[Required]
		public DateTime ActionTimeUtc { get; set; }
	}
}
