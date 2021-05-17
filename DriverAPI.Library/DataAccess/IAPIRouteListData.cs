using DriverAPI.Library.Models;
using System;
using System.Collections.Generic;

namespace DriverAPI.Library.DataAccess
{
    public interface IAPIRouteListData
    {
        APIRouteList Get(int routeListId);
        IEnumerable<APIRouteList> Get(int[] routeListsIds);
        IEnumerable<int> GetRouteListsIdsForDriverByAndroidLogin(string login);
        void RegisterCoordinateForRouteListItem(int routeListAddressId, decimal latitude, decimal longitude, DateTime actionTime);
        string GetActualDriverPushNotificationsTokenByOrderId(int orderId);
    }
}