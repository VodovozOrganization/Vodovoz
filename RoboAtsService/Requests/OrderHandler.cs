using Microsoft.Extensions.Logging;
using RoboAtsService.Monitoring;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Models.Orders;
using Vodovoz.Parameters;

namespace RoboAtsService.Requests
{
	public partial class OrderHandler : GetRequestHandlerBase
	{
		private readonly ILogger<LastOrderHandler> _logger;
		private readonly RoboatsRepository _roboatsRepository;
		private readonly RoboatsOrderModel _roboatsOrderModel;
		private readonly RoboatsSettings _roboatsSettings;
		private readonly RoboatsCallRegistrator _callRegistrator;

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

		public OrderHandler(ILogger<LastOrderHandler> logger, RoboatsRepository roboatsRepository, RoboatsOrderModel roboatsOrderModel, RequestDto requestDto, RoboatsSettings roboatsSettings, RoboatsCallRegistrator callRegistrator) : base(requestDto)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_roboatsOrderModel = roboatsOrderModel ?? throw new ArgumentNullException(nameof(roboatsOrderModel));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_callRegistrator = callRegistrator ?? throw new ArgumentNullException(nameof(callRegistrator));

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
			try
			{
				return ExecuteRequest();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При обработке запроса операций с заказом возникло исключение");
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RoboatsCallFailType.Exception, RoboatsCallOperation.OnOrderHandle,
					$"При обработке запроса информации операций с заказом возникло исключение: {ex.Message}");
				return ErrorMessage;
			}
		}

		public string ExecuteRequest()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			var counterpartyCount = counterpartyIds.Count();
			if(counterpartyCount > 1)
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RoboatsCallFailType.ClientDuplicate, RoboatsCallOperation.OnOrderHandle,
					$"Для телефона {ClientPhone} найдены несколько контрагентов: {string.Join(", ", counterpartyIds)}");
				return ErrorMessage;
			}

			int counterpartyId;
			if(counterpartyCount == 1)
			{
				counterpartyId = counterpartyIds.First();
			}
			else
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RoboatsCallFailType.ClientNotFound, RoboatsCallOperation.OnOrderHandle,
					$"Для телефона {ClientPhone} не найден контрагент");
				return ErrorMessage;
			}

			if(!AddressId.HasValue)
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RoboatsCallFailType.AddressIdNotSpecified, RoboatsCallOperation.OnOrderHandle,
					$"В запросе не указан код точки доставки: {nameof(AddressId)}. Контрагент {counterpartyId}");
				return ErrorMessage;
			}

			var deliveryPointIds = _roboatsRepository.GetLastDeliveryPointIds(counterpartyId);
			if(deliveryPointIds.All(x => x != AddressId))
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RoboatsCallFailType.DeliveryPointsNotFound, RoboatsCallOperation.OnOrderHandle,
					$"Для контрагента {counterpartyId} не найдена точка доставки {AddressId}");
				return ErrorMessage;
			}

			var waters = GetWaters();
			if(!waters.Any())
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RoboatsCallFailType.WaterNotSupported, RoboatsCallOperation.OnOrderHandle,
					$"По указанным в запросе типам воды ({RequestDto.WaterQuantity}) не удалось найти доступную воду. Контрагент {counterpartyId}, точка доставки {AddressId}");
				return ErrorMessage;
			}

			if(!int.TryParse(RequestDto.ReturnBottlesCount, out int bottlesReturn))
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RoboatsCallFailType.AddressIdNotSpecified, RoboatsCallOperation.OnOrderHandle,
					$"В запросе не указано количество бутылей на возврат: {nameof(RequestDto.ReturnBottlesCount)}. Контрагент {counterpartyId}, точка доставки {AddressId}");
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

			_callRegistrator.RegisterTerminatingFail(ClientPhone, RoboatsCallFailType.UnknownRequestType, RoboatsCallOperation.OnOrderHandle,
				$"Неизвестный запрос: RequestType={RequestDto.RequestType}, RequestSubType={RequestDto.RequestSubType}. Контрагент {counterpartyId}, точка доставки {AddressId}");

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
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.NegativeOrderSum, RoboatsCallOperation.CalculateOrderPrice,
					$"При проверке цены, получена отрицательная стоимость заказа. Вода: {RequestDto.WaterQuantity}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}");
				return ErrorMessage;
			}

			var result = (int)Math.Ceiling(price);

			return $"{result}";
		}

		private string CreateOrderAndGetResult(int counterpartyId, int deliveryPointId, IEnumerable<RoboatsWaterInfo> watersInfo, int bottlesReturn)
		{

			if(!DateTime.TryParseExact(RequestDto.Date, "yyyy-MM-dd", new DateTimeFormatInfo(), DateTimeStyles.None, out DateTime date))
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.IncorrectOrderDate, RoboatsCallOperation.CreateOrder,
					$"Некорректная дата. Дата: {RequestDto.Date}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}");
				return ErrorMessage;
			}

			if(!int.TryParse(RequestDto.Time, out int timeId))
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.IncorrectOrderInterval, RoboatsCallOperation.CreateOrder,
					$"Некорректный код интервала доставки. Код: {RequestDto.Time}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}");
				return ErrorMessage;
			}

			var deliverySchedule = _roboatsRepository.GetDeliverySchedule(timeId);
			if(deliverySchedule == null)
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.OrderIntervalNotFound, RoboatsCallOperation.CreateOrder,
					$"Не найден интервал доставки. Код интервала: {timeId}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}");
				return ErrorMessage;
			}

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
					_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.UnknownIsTerminalValue, RoboatsCallOperation.CreateOrder,
						$"Не известный код определения оплаты по терминалу. Код: {RequestDto.IsTerminal}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}");
					return ErrorMessage;
			}

			var isFullOrder = RequestDto.IsFullOrder == "1";

			if(!int.TryParse(RequestDto.BanknoteForReturn, out int banknoteForReturn) && isFullOrder && payment == RoboAtsOrderPayment.Cash)
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.UnknownIsTerminalValue, RoboatsCallOperation.CreateOrder,
					$"Для подтверждения наличного заказа необходимо указать сдачу. Сдача с: {RequestDto.BanknoteForReturn}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}");
				return ErrorMessage;
			}

			var maxBanknoteForReturn = _roboatsSettings.MaxBanknoteForReturn;
			if((banknoteForReturn > maxBanknoteForReturn || banknoteForReturn <= 0) && isFullOrder && payment == RoboAtsOrderPayment.Cash)
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.UnknownIsTerminalValue, RoboatsCallOperation.CreateOrder,
					$"Указанная сдача с, должна быть меньше лимита ({maxBanknoteForReturn}). Сдача с: {RequestDto.BanknoteForReturn}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}");
				return ErrorMessage;
			}

			//Вызов модели создания заказа для создания заказа
			var orderArgs = new RoboatsOrderArgs();
			orderArgs.CounterpartyId = counterpartyId;
			orderArgs.DeliveryPointId = deliveryPointId;
			orderArgs.WatersInfo = watersInfo;
			orderArgs.BottlesReturn = bottlesReturn;
			orderArgs.Date = date;
			orderArgs.DeliveryScheduleId = deliverySchedule.Id;
			orderArgs.PaymentType = payment;
			if(banknoteForReturn > 0)
			{
				orderArgs.BanknoteForReturn = banknoteForReturn;
			}

			try
			{
				if(isFullOrder)
				{
					_roboatsOrderModel.CreateAndAcceptOrder(orderArgs);
					_callRegistrator.RegisterSuccess(ClientPhone);
				}
				else
				{
					_roboatsOrderModel.CreateIncompleteOrder(orderArgs);
					_callRegistrator.RegisterAborted(ClientPhone);
				}
				return "1";
			}
			catch(Exception ex)
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.Exception, RoboatsCallOperation.CreateOrder,
					$"Произошла ошибка при создании заказа. Контрагент {counterpartyId}, точка доставки {deliveryPointId}");
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
			var waters = _roboatsRepository.GetWaterTypes();

			foreach(var waterNode in waterNodes)
			{
				var waterParts = waterNode.Split('-');
				if(waterParts.Length != 2)
				{
					Enumerable.Empty<RoboatsWaterInfo>();
				}


				var waterTypeParsed = int.TryParse(waterParts[0], out int waterTypeId);
				var bottlesCountParsed = int.TryParse(waterParts[1], out int bottlesCount);


				if(!waterTypeParsed || !bottlesCountParsed)
				{
					return Enumerable.Empty<RoboatsWaterInfo>();
				}

				var roboatsWater =  waters.FirstOrDefault(x => x.Id == waterTypeId);
				if(roboatsWater == null)
				{
					return Enumerable.Empty<RoboatsWaterInfo>();
				}

				var waterInfo = new RoboatsWaterInfo(roboatsWater.Nomenclature.Id, bottlesCount);
				result.Add(waterInfo);
			}

			return result;
		}
	}
}
