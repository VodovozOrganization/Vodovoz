using DriverApi.Contracts.V5;
using System.Collections.Generic;

namespace DriverAPI.Library.V5.Services
{
	public interface ITrackPointsService
	{
		void RegisterForRouteList(int routeListId, IList<TrackCoordinateDto> trackList, int driverId);
	}
}
