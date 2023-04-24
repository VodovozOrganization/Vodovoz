using DriverAPI.Library.DTOs;
using System;

namespace DriverAPI.DTOs.V1
{
	public class RollbackRouteListAddressStatusEnRouteRequestDto : IActionTimeTrackable
	{
		public int RoutelistAddressId { get; set; }
		public DateTime? ActionTime { get; set; }
		public DateTime? ActionTimeUtc { get; set; }
	}
}
