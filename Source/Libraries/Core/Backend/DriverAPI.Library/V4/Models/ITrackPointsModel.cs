using DriverApi.Contracts.V4;
using System.Collections.Generic;

namespace DriverAPI.Library.V4.Models
{
	public interface ITrackPointsModel
	{
		void RegisterForRouteList(int routeListId, IList<TrackCoordinateDto> trackList, int driverId);
	}
}
