using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.Converters
{
	public class RouteListConverter
	{
		private readonly ILogger<RouteListConverter> logger;
		private readonly DeliveryPointConverter deliveryPointConverter;
		private readonly RouteListStatusConverter routeListStatusConverter;
		private readonly RouteListAddressStatusConverter routeListAddressStatusConverter;
		private readonly RouteListCompletionStatusConverter routeListCompletionStatusConverter;

		public RouteListConverter(ILogger<RouteListConverter> logger,
			DeliveryPointConverter deliveryPointConverter,
			RouteListStatusConverter routeListStatusConverter,
			RouteListAddressStatusConverter routeListAddressStatusConverter,
			RouteListCompletionStatusConverter routeListCompletionStatusConverter)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.deliveryPointConverter = deliveryPointConverter ?? throw new ArgumentNullException(nameof(deliveryPointConverter));
			this.routeListStatusConverter = routeListStatusConverter ?? throw new ArgumentNullException(nameof(routeListStatusConverter));
			this.routeListAddressStatusConverter = routeListAddressStatusConverter ?? throw new ArgumentNullException(nameof(routeListAddressStatusConverter));
			this.routeListCompletionStatusConverter = routeListCompletionStatusConverter ?? throw new ArgumentNullException(nameof(routeListCompletionStatusConverter));
		}

		public APIRouteList convertToAPIRouteList(RouteList routeList)
		{
			var result = new APIRouteList()
			{
				CompletionStatus = routeListCompletionStatusConverter.convertToAPIRouteListCompletionStatus(routeList.Status)
			};

			if (result.CompletionStatus == APIRouteListCompletionStatus.Completed)
			{
				result.CompletedRouteList = new APICompletedRouteList()
				{
					RouteListId = routeList.Id,
					RouteListStatus = routeListStatusConverter.convertToAPIRouteListStatus(routeList.Status),
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
						catch (ConverterException e)
						{
							logger.LogWarning(e, $"Ошибка конвертирования адреса маршрутного листа {address.Id}");
						}
					}

					result.IncompletedRouteList = new APIIncompletedRouteList()
					{
						RouteListId = routeList.Id,
						RouteListStatus = routeListStatusConverter.convertToAPIRouteListStatus(routeList.Status),
						RouteListAddresses = routelistAddresses
					};
				}
			}

			return result;
		}

		private APIRouteListAddress convertToAPIRouteListAddress(RouteListItem routeListAddress)
		{
			return new APIRouteListAddress()
			{
				Id = routeListAddress.Id,
				Status = routeListAddressStatusConverter.convertToAPIRouteListAddressStatus(routeListAddress.Status),
				DeliveryTime = routeListAddress.Order.DeliveryDate ?? DateTime.MinValue,
				OrderId = routeListAddress.Order.Id,
				FullBottlesCount = routeListAddress.Order.BottlesReturn ?? 0,
				Address = deliveryPointConverter.extractAPIAddressFromDeliveryPoint(routeListAddress.Order.DeliveryPoint)
			};
		}
	}
}
