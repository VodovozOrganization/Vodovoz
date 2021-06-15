using System.Collections.Generic;

namespace DriverAPI.Library.Models
{
	public class APIIncompletedRouteList
	{
		public int RouteListId { get; set; }
		public RouteListDtoStatus RouteListStatus { get; set; }
		public IEnumerable<RouteListAddressDto> RouteListAddresses { get; set; }
	}
}
