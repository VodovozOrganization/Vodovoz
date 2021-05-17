using DriverAPI.Library.Models;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.Converters
{
    public class RouteListAddressStatusConverter
    {
        public APIRouteListAddressStatus convertToAPIRouteListAddressStatus(RouteListItemStatus routeListItemStatus)
        {
            switch (routeListItemStatus)
            {
                case RouteListItemStatus.EnRoute:
                    return APIRouteListAddressStatus.EnRoute;
                case RouteListItemStatus.Completed:
                    return APIRouteListAddressStatus.Completed;
                case RouteListItemStatus.Canceled:
                    return APIRouteListAddressStatus.Canceled;
                case RouteListItemStatus.Overdue:
                    return APIRouteListAddressStatus.Overdue;
                case RouteListItemStatus.Transfered:
                    return APIRouteListAddressStatus.Transfered;
                default:
                    throw new ConverterException(nameof(routeListItemStatus), routeListItemStatus, $"Значение {routeListItemStatus} не поддерживается");
            }
        }
    }
}
