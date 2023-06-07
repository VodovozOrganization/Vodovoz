using System;

namespace DriverAPI.Library.DTOs
{
	public class TrackCoordinateDto
	{
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public DateTime ActionTimeUtc { get; set; }
	}
}
