using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Models.Orders;

namespace RoboAtsService.Requests
{
	public class OrderHandler : GetRequestHandlerBase
	{
		private readonly RoboatsRepository _roboatsRepository;
		private readonly RoboatsOrderModel _roboatsOrderModel;

		public OrderRequestType RequestType { get; }

		public override string Request => RoboatsRequestType.Order;

		public override string ErrorMessage
		{
			get
			{
				switch(RequestType)
				{
					case OrderRequestType.CreateOrder:
						return $"ERROR. order=1&fullorder={RequestDto.IsFullOrder}";
					case OrderRequestType.PriceCheck:
						return $"ERROR. Request=order&show=price";
					case OrderRequestType.Unknown:
					default:
						return $"ERROR. UNKNOWN REQUEST";
				}
			}
		}

		public OrderHandler(RoboatsRepository roboatsRepository, RoboatsOrderModel roboatsOrderModel, RequestDto requestDto) : base(requestDto)
		{
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_roboatsOrderModel = roboatsOrderModel ?? throw new ArgumentNullException(nameof(roboatsOrderModel));

			if(RequestDto.RequestSubType == "price")
			{
				RequestType = OrderRequestType.PriceCheck;
			}
			else if(RequestDto.IsAddOrder == "1")
			{
				RequestType = OrderRequestType.CreateOrder;
			}
			else
			{
				RequestType = OrderRequestType.Unknown;
			}
		}

		public override string Execute()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			var counterpartyCount = counterpartyIds.Count();
			if(counterpartyCount > 1)
			{
				return ErrorMessage;
			}
			var counterpartyId = counterpartyIds.First();

			var deliveryPointIds = _roboatsRepository.GetLastDeliveryPointIds(counterpartyId);
			if(!AddressId.HasValue || deliveryPointIds.All(x => x != AddressId))
			{
				return ErrorMessage;
			}

			var waters = GetWaters();
			if(!waters.Any())
			{
				return ErrorMessage;
			}

			if(!int.TryParse(RequestDto.ReturnBottlesCount, out int bottlesReturn))
			{
				return ErrorMessage;
			}

			if(RequestDto.RequestSubType == "price")
			{
				return CalculatePrice(counterpartyId, AddressId.Value, waters, bottlesReturn);
			}

			if(RequestDto.IsAddOrder == "1")
			{
				return CreateOrderAndGetResult(counterpartyId, AddressId.Value, waters, bottlesReturn);
			}

			return ErrorMessage;
		}

		private string CalculatePrice(int counterpartyId, int deliveryPointId, IEnumerable<RoboatsWaterInfo> watersInfo, int bottlesReturn)
		{
			var orderArgs = new RoboatsOrderArgs();
			orderArgs.CounterpartyId = counterpartyId;
			orderArgs.DeliveryPointId = deliveryPointId;
			orderArgs.WatersInfo = watersInfo;
			orderArgs.BottlesReturn = bottlesReturn;

			var price = _roboatsOrderModel.GetOrderPrice(orderArgs);
			if(price <= 0)
			{
				return ErrorMessage;
			}

			var result = (int)Math.Ceiling(price);

			return $"{result}";
		}

		private string CreateOrderAndGetResult(int counterpartyId, int deliveryPointId, IEnumerable<RoboatsWaterInfo> watersInfo, int bottlesReturn)
		{

			if(!DateTime.TryParseExact(RequestDto.Date, "yyyy-MM-dd", new DateTimeFormatInfo(), DateTimeStyles.None, out DateTime date))
			{
				return ErrorMessage;
			}

			if(!int.TryParse(RequestDto.Time, out int timeId))
			{
				return ErrorMessage;
			}

			var deliverySchedule = _roboatsRepository.GetDeliverySchedule(timeId);

			RoboAtsOrderPayment payment;
			switch(RequestDto.IsTerminal)
			{
				case "1":
					payment = RoboAtsOrderPayment.Terminal;
					break;
				case "0":
					payment = RoboAtsOrderPayment.Cash;
					break;
				default:
					return ErrorMessage;
			}

			if(!int.TryParse(RequestDto.BanknoteForReturn, out int banknoteForReturn))
			{
				return ErrorMessage;
			}

			if(banknoteForReturn > 10000 || banknoteForReturn <= 0)
			{
				return ErrorMessage;
			}

			var isFullOrder = RequestDto.IsFullOrder == "1";

			//Вызов модели создания заказа для создания заказа
			var orderArgs = new RoboatsOrderArgs();
			orderArgs.CounterpartyId = counterpartyId;
			orderArgs.DeliveryPointId = deliveryPointId;
			orderArgs.WatersInfo = watersInfo;
			orderArgs.BottlesReturn = bottlesReturn;
			orderArgs.Date = date;
			orderArgs.DeliveryScheduleId = deliverySchedule.Id;
			orderArgs.PaymentType = payment;
			orderArgs.BanknoteForReturn = banknoteForReturn;

			try
			{
				if(isFullOrder)
				{
					_roboatsOrderModel.CreateAndAcceptOrder(orderArgs);
				}
				else
				{
					_roboatsOrderModel.CreateIncompleteOrder(orderArgs);
				}
				return "1";
			}
			catch(Exception ex)
			{
				return ErrorMessage;
			}
		}

		private IEnumerable<RoboatsWaterInfo> GetWaters()
		{
			var waterNodes = RequestDto.WaterQuantity.Split('|');
			if(!waterNodes.Any())
			{
				return Enumerable.Empty<RoboatsWaterInfo>();
			}

			var result = new List<RoboatsWaterInfo>();
			var waters = _roboatsRepository.GetAvailableWaters();

			foreach(var waterNode in waterNodes)
			{
				var waterParts = waterNode.Split('-');
				if(waterParts.Length != 2)
				{
					Enumerable.Empty<RoboatsWaterInfo>();
				}

				var waterTypeParsed = int.TryParse(waterParts[0], out int waterType);
				var bottlesCountParsed = int.TryParse(waterParts[1], out int bottlesCount);

				if(!waterTypeParsed || !bottlesCountParsed)
				{
					return Enumerable.Empty<RoboatsWaterInfo>();
				}

				var waterInfo = new RoboatsWaterInfo(waterType, bottlesCount);
				result.Add(waterInfo);
			}

			return result;
		}



		public enum OrderRequestType
		{
			Unknown,
			CreateOrder,
			PriceCheck
		}


	}
}
