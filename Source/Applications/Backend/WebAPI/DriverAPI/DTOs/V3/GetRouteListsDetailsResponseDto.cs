using DriverAPI.Library.DTOs;
using System.Collections.Generic;

namespace DriverAPI.DTOs.V3
{
	/// <summary>
	/// Детализированная информация по маршрутным листам
	/// </summary>
	public class GetRouteListsDetailsResponseDto
	{
		/// <summary>
		/// Маршрутные листы
		/// </summary>
		public IEnumerable<RouteListDto> RouteLists { get; set; }

		/// <summary>
		/// Заказы
		/// </summary>
		public IEnumerable<OrderDto> Orders { get; set; }
	}
}
