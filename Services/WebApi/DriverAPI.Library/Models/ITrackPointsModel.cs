using DriverAPI.Library.DTOs;
using System.Collections.Generic;

namespace DriverAPI.Library.Models
{
	public interface ITrackPointsModel
	{
		void RegisterForRouteList(int routeListId, IList<TrackCoordinateDto> trackList, int driverId);
	}
}
