using System;

namespace DriverAPI.DTOs.V2
{
	public class RouteListAddressCoordinateDto
	{
		public int RouteListAddressId { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public DateTime ActionTimeUtc { get; set; }
	}
}
