using DriverApi.Contracts.V6;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;

namespace DriverAPI.Library.V6.Converters
{
	/// <summary>
	/// Конвертер маршрутного листа
	/// </summary>
	public class RouteListConverter
	{
		private readonly DeliveryPointConverter _deliveryPointConverter;
		private readonly RouteListStatusConverter _routeListStatusConverter;
		private readonly RouteListAddressStatusConverter _routeListAddressStatusConverter;
		private readonly RouteListCompletionStatusConverter _routeListCompletionStatusConverter;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="deliveryPointConverter"></param>
		/// <param name="routeListStatusConverter"></param>
		/// <param name="routeListAddressStatusConverter"></param>
		/// <param name="routeListCompletionStatusConverter"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public RouteListConverter(
			DeliveryPointConverter deliveryPointConverter,
			RouteListStatusConverter routeListStatusConverter,
			RouteListAddressStatusConverter routeListAddressStatusConverter,
			RouteListCompletionStatusConverter routeListCompletionStatusConverter)
		{
			_deliveryPointConverter = deliveryPointConverter ?? throw new ArgumentNullException(nameof(deliveryPointConverter));
			_routeListStatusConverter = routeListStatusConverter ?? throw new ArgumentNullException(nameof(routeListStatusConverter));
			_routeListAddressStatusConverter = routeListAddressStatusConverter ?? throw new ArgumentNullException(nameof(routeListAddressStatusConverter));
			_routeListCompletionStatusConverter = routeListCompletionStatusConverter ?? throw new ArgumentNullException(nameof(routeListCompletionStatusConverter));
		}

		/// <summary>
		/// Метод конвертации в DTO
		/// </summary>
		/// <param name="routeList">Маршрутный лист ДВ</param>
		/// <param name="itemsToReturn">Оборудование на возврат</param>
		/// <returns></returns>
		public RouteListDto ConvertToAPIRouteList(RouteList routeList, IEnumerable<KeyValuePair<string, int>> itemsToReturn, IEnumerable<KeyValuePair<int, string>> spectiaConditionsToAccept)
		{
			var result = new RouteListDto()
			{
				ForwarderFullName = routeList.Forwarder?.FullName ?? "Нет",
				CompletionStatus = _routeListCompletionStatusConverter.ConvertToAPIRouteListCompletionStatus(routeList.Status)
			};

			if(result.CompletionStatus == RouteListDtoCompletionStatus.Completed)
			{
				var fullBottlesToReturn = routeList.ObservableDeliveryFreeBalanceOperations
					.Where(x => x.Nomenclature.IsWater19L)
					.Sum(x => x.Amount);

				result.CompletedRouteList = new CompletedRouteListDto()
				{
					RouteListId = routeList.Id,
					RouteListStatus = _routeListStatusConverter.ConvertToAPIRouteListStatus(routeList.Status),
					CashMoney = routeList.Addresses
						.Where(rla => rla.Status == RouteListItemStatus.Completed
							&& rla.Order.PaymentType == Vodovoz.Domain.Client.PaymentType.Cash)
						.Sum(rla => rla.Order.OrderSum),
					TerminalCardMoney = routeList.Addresses
						.Where(rla => rla.Status == RouteListItemStatus.Completed
							&& rla.Order.PaymentType == Vodovoz.Domain.Client.PaymentType.Terminal
							&& rla.Order.PaymentByTerminalSource == Vodovoz.Domain.Client.PaymentByTerminalSource.ByCard)
						.Sum(rla => rla.Order.OrderSum),
					TerminalQRMoney = routeList.Addresses
						.Where(rla => rla.Status == RouteListItemStatus.Completed
							&& rla.Order.PaymentType == Vodovoz.Domain.Client.PaymentType.Terminal
							&& rla.Order.PaymentByTerminalSource == Vodovoz.Domain.Client.PaymentByTerminalSource.ByQR)
						.Sum(rla => rla.Order.OrderSum),
					TerminalOrdersCount = routeList.Addresses
						.Where(rla => rla.Status == RouteListItemStatus.Completed
							&& rla.Order.PaymentType == Vodovoz.Domain.Client.PaymentType.Terminal)
						.Count(),
					FullBottlesToReturn = (int)fullBottlesToReturn,
					EmptyBottlesToReturn = routeList.Addresses
						.Sum(rla => rla.DriverBottlesReturned ?? 0),
				};

				result.CompletedRouteList.OrdersReturnItems = itemsToReturn.Select(pair => new OrdersReturnItemDto() { Name = pair.Key, Count = pair.Value });
			}
			else
			{
				if(result.CompletionStatus == RouteListDtoCompletionStatus.Incompleted)
				{
					var routelistAddresses = new List<RouteListAddressDto>();

					foreach(var address in routeList.Addresses.OrderBy(address => address.IndexInRoute))
					{
						routelistAddresses.Add(ConvertToAPIRouteListAddress(address));
					}

					result.IncompletedRouteList = new IncompletedRouteListDto()
					{
						RouteListId = routeList.Id,
						RouteListStatus = _routeListStatusConverter.ConvertToAPIRouteListStatus(routeList.Status),
						RouteListAddresses = routelistAddresses,
						SpecialConditionsToAccept = ConvertToApiDto(spectiaConditionsToAccept)
					};
				}
			}

			return result;
		}

		private RouteListAddressDto ConvertToAPIRouteListAddress(RouteListItem routeListAddress)
		{
			return new RouteListAddressDto()
			{
				Id = routeListAddress.Id,
				Status = _routeListAddressStatusConverter.ConvertToAPIRouteListAddressStatus(routeListAddress.Status),
				DeliveryIntervalStart = routeListAddress.Order.DeliveryDate + routeListAddress.Order.DeliverySchedule.From ?? DateTime.MinValue,
				DeliveryIntervalEnd = routeListAddress.Order.DeliveryDate + routeListAddress.Order.DeliverySchedule.To ?? DateTime.MinValue,
				OrderId = routeListAddress.Order.Id,
				Address = _deliveryPointConverter.ExtractAPIAddressFromDeliveryPoint(routeListAddress.Order.DeliveryPoint)
			};
		}

		private IEnumerable<RouteListSpecialConditionDto> ConvertToApiDto(IEnumerable<KeyValuePair<int, string>> specialConditions)
		{

			foreach(var specialConditon in specialConditions)
			{
				yield return new RouteListSpecialConditionDto
				{
					Id = specialConditon.Key,
					Name = specialConditon.Value
				};
			}
		}
	}
}
