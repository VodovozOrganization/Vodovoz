using DriverAPI.Library.DTOs;
using System.Collections.Generic;

namespace DriverAPI.DTOs.V1
{
	public class RegisterTrackCoordinateRequestDto
	{
		public int RouteListId { get; set; }
		public IEnumerable<TrackCoordinateDto> TrackList { get; set; }
	}
}
