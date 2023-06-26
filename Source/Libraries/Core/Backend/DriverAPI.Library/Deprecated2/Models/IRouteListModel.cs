using DriverAPI.Library.Deprecated2.DTOs;
using System;
using System.Collections.Generic;

namespace DriverAPI.Library.Deprecated2.Models
{
	[Obsolete("Будет удален с прекращением поддержки API v2")]
	public interface IRouteListModel
	{
		RouteListDto Get(int routeListId);
		IEnumerable<RouteListDto> Get(int[] routeListsIds);
		IEnumerable<int> GetRouteListsIdsForDriverByAndroidLogin(string login);
		string GetActualDriverPushNotificationsTokenByOrderId(int orderId);
		void RollbackRouteListAddressStatusEnRoute(int routeListAddressId, int id);
		bool IsRouteListBelongToDriver(int routeListId, int driverId);
		void RegisterCoordinateForRouteListItem(int routeListAddressId, decimal latitude, decimal longitude, DateTime actionTime, int driverId);
	}
}
