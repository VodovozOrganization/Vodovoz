using DriverAPI.Library.Models;
using System.Collections.Generic;

namespace DriverAPI.Models
{
	public class RegisterTrackCoordinateRequestModel
	{
		public int RouteListId { get; set; }
		public IEnumerable<APITrackCoordinate> TrackList { get; set; }
	}
}
