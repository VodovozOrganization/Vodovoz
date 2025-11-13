using System;
using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Запрос на регистрацию предполагаемого адреса точки доставки
	/// </summary>
	public class RouteListAddressCoordinateDto
	{
		/// <summary>
		/// Номер адреса маршрутного листа
		/// </summary>
		[Required]
		public int RouteListAddressId { get; set; }

		/// <summary>
		/// Широта
		/// </summary>
		[Required]
		public decimal Latitude { get; set; }

		/// <summary>
		/// Долгота
		/// </summary>
		[Required]
		public decimal Longitude { get; set; }

		/// <summary>
		/// Время операции на стороне мобильного приложения водителя
		/// </summary>
		[Required]
		public DateTime ActionTimeUtc { get; set; }
	}
}
