using DriverAPI.Library.Deprecated.DTOs;
using DriverAPI.Library.DTOs;
using System.Collections.Generic;

namespace DriverAPI.DTOs.V1
{
	public class GetRouteListsDetailsResponseDto
	{
		public IEnumerable<RouteListDto> RouteLists { get; set; }
		public IEnumerable<OrderDto> Orders { get; set; }
	}
}
