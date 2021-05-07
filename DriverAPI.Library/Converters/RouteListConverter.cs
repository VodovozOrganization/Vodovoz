using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.Converters
{
    public class RouteListConverter
    {
        private readonly ILogger<RouteListConverter> logger;
        private readonly DeliveryPointConverter deliveryPointConverter;

        public RouteListConverter(ILogger<RouteListConverter> logger, DeliveryPointConverter deliveryPointConverter)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.deliveryPointConverter = deliveryPointConverter ?? throw new ArgumentNullException(nameof(deliveryPointConverter));
        }

        public APIRouteList convertToAPIRouteList(RouteList routeList)
        {
            var result = new APIRouteList()
            {
                CompletionStatus = convertToAPIRouteListCompletionStatus(routeList.Status)
            };

            

            if (result.CompletionStatus == APIRouteListCompletionStatus.Completed)
            {
                result.CompletedRouteList = new APICompletedRouteList()
                {
                    RouteListId = routeList.Id,
                    RouteListStatus = convertToAPIStatus(routeList.Status),
                    CashMoney = routeList.Addresses
                        .Where(rla => rla.Status == RouteListItemStatus.Completed 
                            && rla.Order.PaymentType == Vodovoz.Domain.Client.PaymentType.cash)
                        .Sum(rla => rla.Order.ActualTotalSum),
                    TerminalMoney = routeList.Addresses
                        .Where(rla => rla.Status == RouteListItemStatus.Completed
                            && rla.Order.PaymentType == Vodovoz.Domain.Client.PaymentType.Terminal)
                        .Sum(rla => rla.Order.ActualTotalSum),
                    TerminalOrdersCount = routeList.Addresses
                        .Where(rla => rla.Status == RouteListItemStatus.Completed
                            && rla.Order.PaymentType == Vodovoz.Domain.Client.PaymentType.Terminal)
                        .Count(),
                    FullBottlesToReturn = routeList.Addresses
                        .Where(rla => rla.Status == RouteListItemStatus.Canceled 
                            || rla.Status == RouteListItemStatus.Overdue)
                        .Sum(rla => rla.Order.Total19LBottlesToDeliver),
                    EmptyBottlesToReturn = routeList.Addresses
                        .Sum(rla => rla.Order.BottlesReturn ?? 0),
                };
            }
            else
            {
                if (result.CompletionStatus == APIRouteListCompletionStatus.Incompleted)
                {
                    var routelistAddresses = new List<APIRouteListAddress>();

                    foreach (var address in routeList.Addresses)
                    {
                        try
                        {
                            routelistAddresses.Add(convertToAPIRouteListAddress(address));
                        }
                        catch (ArgumentException e)
                        {
                            logger.LogWarning(e, $"Ошибка конвертирования адреса маршрутного листа {address.Id}");
                        }
                    }

                    result.IncompletedRouteList = new APIIncompletedRouteList()
                    {
                        RouteListId = routeList.Id,
                        RouteListStatus = convertToAPIStatus(routeList.Status),
                        RouteListAddresses = routelistAddresses
                    };
                }
            }

            return result;
        }

        private APIRouteListStatus convertToAPIStatus(RouteListStatus routeListStatus)
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
                    logger.LogWarning($"Не поддерживается тип: {routeListStatus}");
                    throw new ArgumentException($"Не поддерживается тип: {routeListStatus}");
            }
        }

        private APIRouteListCompletionStatus convertToAPIRouteListCompletionStatus(RouteListStatus routeListStatus)
        {
            switch (routeListStatus)
            {
                case RouteListStatus.New:
                case RouteListStatus.Confirmed:
                case RouteListStatus.InLoading:
                case RouteListStatus.EnRoute:
                    return APIRouteListCompletionStatus.Incompleted;
                case RouteListStatus.Delivered:
                case RouteListStatus.OnClosing:
                case RouteListStatus.MileageCheck:
                case RouteListStatus.Closed:
                    return APIRouteListCompletionStatus.Completed;
                default:
                    logger.LogWarning($"Не поддерживается тип: {routeListStatus}");
                    throw new ArgumentException($"Не поддерживается тип: {routeListStatus}");
            }
        }

        private APIRouteListAddressStatus convertToAPIRouteListAddressStatus(RouteListItemStatus routeListItemStatus)
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
                    logger.LogWarning($"Не поддерживается тип: {routeListItemStatus}");
                    throw new ArgumentException($"Не поддерживается тип: {routeListItemStatus}");
            }
        }

        private APIRouteListAddress convertToAPIRouteListAddress(RouteListItem routeListAddress)
        {
            return new APIRouteListAddress()
            {
                Id = routeListAddress.Id,
                Status = convertToAPIRouteListAddressStatus(routeListAddress.Status),
                DeliveryTime = routeListAddress.Order.DeliveryDate ?? DateTime.MinValue,
                OrderId = routeListAddress.Order.Id,
                FullBottlesCount = routeListAddress.Order.BottlesReturn ?? 0,
                Address = deliveryPointConverter.extractAPIAddressFromDeliveryPoint(routeListAddress.Order.DeliveryPoint)
            };
        }
    }
}
