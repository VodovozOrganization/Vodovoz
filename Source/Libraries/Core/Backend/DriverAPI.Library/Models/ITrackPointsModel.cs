using DriverApi.Contracts.V4;
using System.Collections.Generic;

namespace DriverAPI.Library.Models
{
	public interface ITrackPointsModel
	{
		void RegisterForRouteList(int routeListId, IList<TrackCoordinateDto> trackList, int driverId);
	}
}
