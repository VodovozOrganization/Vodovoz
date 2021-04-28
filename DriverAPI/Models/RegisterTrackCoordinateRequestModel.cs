using System.Collections.Generic;

namespace DriverAPI.Models
{
    public class RegisterTrackCoordinateRequestModel
    {
        public int RouteListId { get; set; }
        public IEnumerable<TrackCoordinate> TrackList { get; set; }
    }
}
