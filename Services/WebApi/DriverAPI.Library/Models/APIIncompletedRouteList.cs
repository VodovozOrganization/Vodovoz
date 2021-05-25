using System.Collections.Generic;

namespace DriverAPI.Library.Models
{
	public class APIIncompletedRouteList
	{
		public int RouteListId { get; set; }
		public APIRouteListStatus RouteListStatus { get; set; }
		public IEnumerable<APIRouteListAddress> RouteListAddresses { get; set; }
	}
}
