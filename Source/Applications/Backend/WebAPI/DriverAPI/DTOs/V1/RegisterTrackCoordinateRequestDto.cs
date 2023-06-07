using System.Collections.Generic;
using TrackCoordinateDto = DriverAPI.Library.Deprecated.DTOs.TrackCoordinateDto;

namespace DriverAPI.DTOs.V1
{
	public class RegisterTrackCoordinateRequestDto
	{
		public int RouteListId { get; set; }
		public IEnumerable<TrackCoordinateDto> TrackList { get; set; }
	}
}
