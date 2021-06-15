using DriverAPI.Library.Models;
using System.Collections.Generic;

namespace DriverAPI.Models
{
	public class GetRouteListsDetailsResponseModel
	{
		public IEnumerable<RouteListDto> RouteLists { get; set; }
		public IEnumerable<OrderDto> Orders { get; set; }
	}
}
