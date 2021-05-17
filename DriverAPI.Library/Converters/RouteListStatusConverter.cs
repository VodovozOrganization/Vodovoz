using DriverAPI.Library.Models;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.Converters
{
    public class RouteListStatusConverter
    {
        public APIRouteListStatus convertToAPIRouteListStatus(RouteListStatus routeListStatus)
        {
            switch (routeListStatus)
            {
                case RouteListStatus.New:
                    return APIRouteListStatus.New;
                case RouteListStatus.Confirmed:
                    return APIRouteListStatus.Confirmed;
                case RouteListStatus.InLoading:
                    return APIRouteListStatus.InLoading;
                case RouteListStatus.EnRoute:
                    return APIRouteListStatus.EnRoute;
                case RouteListStatus.Delivered:
                    return APIRouteListStatus.Delivered;
                case RouteListStatus.OnClosing:
                    return APIRouteListStatus.OnClosing;
                case RouteListStatus.MileageCheck:
                    return APIRouteListStatus.MileageCheck;
                case RouteListStatus.Closed:
                    return APIRouteListStatus.Closed;
                default:
                    throw new ConverterException(nameof(routeListStatus), routeListStatus, $"Значение {routeListStatus} не поддерживается");
            }
        }
    }
}
