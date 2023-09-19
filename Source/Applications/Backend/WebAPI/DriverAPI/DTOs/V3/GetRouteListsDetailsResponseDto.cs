using DriverAPI.Library.DTOs;
using System.Collections.Generic;
using RouteListDto = DriverAPI.Library.Deprecated3.DTOs.RouteListDto;

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
