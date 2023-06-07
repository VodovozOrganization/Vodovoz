using System;

namespace DriverAPI.DTOs.V2
{
	public class RollbackRouteListAddressStatusEnRouteRequestDto
	{
		public int RoutelistAddressId { get; set; }
		public DateTime ActionTimeUtc { get; set; }
	}
}
