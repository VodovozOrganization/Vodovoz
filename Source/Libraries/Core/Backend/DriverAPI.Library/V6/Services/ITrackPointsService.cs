using DriverApi.Contracts.V6;
using System.Collections.Generic;

namespace DriverAPI.Library.V6.Services
{
	public interface ITrackPointsService
	{
		void RegisterForRouteList(int routeListId, IList<TrackCoordinateDto> trackList, int driverId);
	}
}
