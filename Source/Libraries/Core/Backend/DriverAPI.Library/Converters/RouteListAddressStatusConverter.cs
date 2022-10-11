using DriverAPI.Library.DTOs;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.Converters
{
	public class RouteListAddressStatusConverter
	{
		public RouteListAddressDtoStatus convertToAPIRouteListAddressStatus(RouteListItemStatus routeListItemStatus)
		{
			switch (routeListItemStatus)
			{
				case RouteListItemStatus.EnRoute:
					return RouteListAddressDtoStatus.EnRoute;
				case RouteListItemStatus.Completed:
					return RouteListAddressDtoStatus.Completed;
				case RouteListItemStatus.Canceled:
					return RouteListAddressDtoStatus.Canceled;
				case RouteListItemStatus.Overdue:
					return RouteListAddressDtoStatus.Overdue;
				case RouteListItemStatus.Transfered:
					return RouteListAddressDtoStatus.Transfered;
				default:
					throw new ConverterException(nameof(routeListItemStatus), routeListItemStatus, $"Значение { routeListItemStatus } не поддерживается");
			}
		}
	}
}
