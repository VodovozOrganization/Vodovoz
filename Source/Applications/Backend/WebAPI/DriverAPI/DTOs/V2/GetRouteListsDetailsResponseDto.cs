using DriverAPI.Library.DTOs;
using System.Collections.Generic;
using RouteListDto = DriverAPI.Library.Deprecated2.DTOs.RouteListDto;
using OrderDto = DriverAPI.Library.Deprecated2.DTOs.OrderDto;

namespace DriverAPI.DTOs.V2
{
	public class GetRouteListsDetailsResponseDto
	{
		public IEnumerable<RouteListDto> RouteLists { get; set; }
		public IEnumerable<OrderDto> Orders { get; set; }
	}
}
