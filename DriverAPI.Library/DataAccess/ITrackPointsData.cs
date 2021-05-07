using DriverAPI.Library.Models;
using System.Collections.Generic;

namespace DriverAPI.Library.DataAccess
{
    public interface ITrackPointsData
    {
        void RegisterForRouteList(int routeListId, IEnumerable<APITrackCoordinate> trackList);
    }
}