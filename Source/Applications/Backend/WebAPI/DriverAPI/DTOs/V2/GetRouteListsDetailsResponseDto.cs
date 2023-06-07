using DriverAPI.Library.DTOs;
using System.Collections.Generic;

namespace DriverAPI.DTOs.V2
{
	public class GetRouteListsDetailsResponseDto
	{
		public IEnumerable<RouteListDto> RouteLists { get; set; }
		public IEnumerable<OrderDto> Orders { get; set; }
	}
}
