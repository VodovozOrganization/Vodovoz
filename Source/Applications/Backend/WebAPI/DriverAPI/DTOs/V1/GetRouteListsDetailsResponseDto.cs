using DriverAPI.Library.DTOs;
using System.Collections.Generic;
using OrderDto = DriverAPI.Library.Deprecated.DTOs.OrderDto;
using RouteListDto = DriverAPI.Library.Deprecated2.DTOs.RouteListDto;

namespace DriverAPI.DTOs.V1
{
	public class GetRouteListsDetailsResponseDto
	{
		public IEnumerable<RouteListDto> RouteLists { get; set; }
		public IEnumerable<OrderDto> Orders { get; set; }
	}
}
