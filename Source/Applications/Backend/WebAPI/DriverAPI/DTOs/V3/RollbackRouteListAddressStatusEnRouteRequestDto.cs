using System;

namespace DriverAPI.DTOs.V3
{
	public class RollbackRouteListAddressStatusEnRouteRequestDto
	{
		public int RoutelistAddressId { get; set; }
		public DateTime ActionTimeUtc { get; set; }
	}
}
