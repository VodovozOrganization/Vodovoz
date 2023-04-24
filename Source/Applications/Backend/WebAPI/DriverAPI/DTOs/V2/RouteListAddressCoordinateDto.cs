using DriverAPI.Library.DTOs;
using System;

namespace DriverAPI.DTOs.V2
{
	public class RouteListAddressCoordinateDto : IActionTimeTrackable
	{
		public int RouteListAddressId { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public DateTime? ActionTime { get; set; }
		public DateTime? ActionTimeUtc { get; set; }
	}
}
