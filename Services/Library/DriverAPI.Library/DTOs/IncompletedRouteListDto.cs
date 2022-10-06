using System.Collections.Generic;

namespace DriverAPI.Library.DTOs
{
	public class IncompletedRouteListDto
	{
		public int RouteListId { get; set; }
		public RouteListDtoStatus RouteListStatus { get; set; }
		public IEnumerable<RouteListAddressDto> RouteListAddresses { get; set; }
	}
}
