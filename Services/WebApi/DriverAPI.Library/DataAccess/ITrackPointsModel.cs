using DriverAPI.Library.DTOs;
using System.Collections.Generic;

namespace DriverAPI.Library.DataAccess
{
	public interface ITrackPointsModel
	{
		void RegisterForRouteList(int routeListId, IEnumerable<TrackCoordinateDto> trackList);
	}
}