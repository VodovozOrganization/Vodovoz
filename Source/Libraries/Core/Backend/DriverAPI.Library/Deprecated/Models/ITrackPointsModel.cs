using System;
using System.Collections.Generic;
using TrackCoordinateDto = DriverAPI.Library.Deprecated.DTOs.TrackCoordinateDto;

namespace DriverAPI.Library.Deprecated.Models
{
	[Obsolete("Будет удален с прекращением поддержки API v1")]
	public interface ITrackPointsModel
	{
		void RegisterForRouteList(int routeListId, IList<TrackCoordinateDto> trackList, int driverId);
	}
}
