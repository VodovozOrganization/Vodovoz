using DriverAPI.Library.Models;
using System.Collections.Generic;

namespace DriverAPI.Library.DataAccess
{
    public interface IAPIRouteListData
    {
        APIRouteList Get(int routeListId);
        IEnumerable<APIRouteList> Get(int[] routeListsIds);
    }
}