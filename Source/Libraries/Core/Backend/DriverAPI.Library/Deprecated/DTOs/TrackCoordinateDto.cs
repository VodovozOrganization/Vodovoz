using DriverAPI.Library.Deprecated.DTOs;
using System;

namespace DriverAPI.Library.DTOs
{
	public class TrackCoordinateDto : IActionTimeTrackable
	{
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		public DateTime? ActionTime { get; set; }
		public DateTime? ActionTimeUtc { get; set; }
	}
}
