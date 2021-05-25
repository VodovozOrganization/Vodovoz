using DriverAPI.Library.Models;
using System.Collections.Generic;

namespace DriverAPI.Models
{
	public class GetRouteListsDetailsResponseModel
	{
		public IEnumerable<APIRouteList> RouteLists { get; set; }
		public IEnumerable<APIOrder> Orders { get; set; }
	}
}
