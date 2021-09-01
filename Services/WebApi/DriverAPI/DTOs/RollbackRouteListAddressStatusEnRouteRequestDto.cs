using System;

namespace DriverAPI.DTOs
{
	public class RollbackRouteListAddressStatusEnRouteRequestDto : IDelayedAction
	{
		public int RoutelistAddressId { get; set; }
		public DateTime ActionTime { get; set; }
	}
}