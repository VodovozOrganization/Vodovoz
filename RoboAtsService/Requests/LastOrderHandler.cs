using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Counterparties;

namespace RoboAtsService.Requests
{
	/// <summary>
	/// Обработчик запросов данных о заказе
	/// </summary>
	public class LastOrderHandler : GetRequestHandlerBase
	{
		private readonly RoboatsRepository _roboatsRepository;

		public LastOrderRequestType RequestType { get; }

		public override string Request => RoboatsRequestType.LastOrder;

		public LastOrderHandler(RoboatsRepository roboatsRepository, RequestDto requestDto) : base(requestDto)
		{
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));

			switch(RequestDto.RequestSubType)
			{
				case "order_id":
					RequestType = LastOrderRequestType.GetLastOrderId;
					break;
				case "waterquantity":
					RequestType = LastOrderRequestType.WaterQuantity;
					break;
				case "return":
					RequestType = LastOrderRequestType.BottlesReturn;
					break;
				default:
					RequestType = LastOrderRequestType.LastOrderExist;
					break;
			}
		}

		public override string Execute()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			if(counterpartyIds.Count() != 1)
			{
				return ErrorMessage;
			}

			var counterpartyId = counterpartyIds.First();

			switch(RequestType)
			{
				case LastOrderRequestType.LastOrderExist:
					return GetLastOrderCheck(counterpartyId);
				case LastOrderRequestType.GetLastOrderId:
					return GetLastOrderId(counterpartyId);
				case LastOrderRequestType.WaterQuantity:
					return GetWaterInfo(counterpartyId);
				case LastOrderRequestType.BottlesReturn:
					return GetBottlesReturn(counterpartyId);
				default:
					return ErrorMessage;
			}
		}

		private string GetLastOrderCheck(int counterpartyId)
		{
			var order = _roboatsRepository.GetLastOrder(counterpartyId);
			return order != null ? "1" : "0";
		}

		private string GetLastOrderId(int counterpartyId)
		{
			var deliveryPointIds = _roboatsRepository.GetLastDeliveryPointIds(counterpartyId);
			if(!AddressId.HasValue || deliveryPointIds.All(x => x != AddressId))
			{
				return ErrorMessage;
			}

			var order = _roboatsRepository.GetLastOrder(counterpartyId, AddressId.Value);
			if(order == null)
			{
				return "NO DATA";
			}

			return $"{order.Id}";
		}

		private string GetWaterInfo(int counterpartyId)
		{
			if(!int.TryParse(RequestDto.OrderId, out int orderId))
			{
				return ErrorMessage;
			}

			var availableWaters = _roboatsRepository.GetAvailableWaters();
			if(!availableWaters.Any())
			{
				return ErrorMessage;
			}

			var waterQuantity = _roboatsRepository.GetWatersQuantityFromOrder(counterpartyId, orderId);
			if(!waterQuantity.Any())
			{
				return ErrorMessage;
			}

			List<string> results = new List<string>();
			foreach(var waterItem in waterQuantity)
			{
				var roboatsWaterInfo = availableWaters.FirstOrDefault(x => x.Nomenclature.Id == waterItem.NomenclatureId);
				if(roboatsWaterInfo == null)
				{
					return ErrorMessage;
				}

				results.Add($"{roboatsWaterInfo.RoboatsId}-{waterItem.Quantity}");
			}

			var result = string.Join('|', results);

			return result;
		}

		private string GetBottlesReturn(int counterpartyId)
		{
			if(!int.TryParse(RequestDto.OrderId, out int orderId))
			{
				return ErrorMessage;
			}

			var bottlesReturn = _roboatsRepository.GetBottlesReturnForOrder(counterpartyId, orderId);
			if(!bottlesReturn.HasValue)
			{
				return ErrorMessage;
			}

			return $"{bottlesReturn.Value}";
		}
	}

	public enum LastOrderRequestType
	{
		LastOrderExist,
		GetLastOrderId,
		WaterQuantity,
		BottlesReturn
	}
}
