using Microsoft.Extensions.Logging;
using RoboatsService.Monitoring;
using RoboatsService.OrderValidation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Models.Orders;
using Vodovoz.Parameters;

namespace RoboatsService.Requests
{
	public partial class OrderHandler : GetRequestHandlerBase
	{
		private readonly ILogger<LastOrderHandler> _logger;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IOrderService _orderService;
		private readonly ValidOrdersProvider _validOrdersProvider;
		private readonly IRoboatsSettings _roboatsSettings;
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

		public OrderHandler(
			ILogger<LastOrderHandler> logger,
			IRoboatsRepository roboatsRepository,
			IOrderService orderService,
			ValidOrdersProvider validOrdersProvider,
			RequestDto requestDto,
			IRoboatsSettings roboatsSettings,
			RoboatsCallRegistrator callRegistrator) : base(requestDto)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
			_validOrdersProvider = validOrdersProvider ?? throw new ArgumentNullException(nameof(validOrdersProvider));
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
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.Exception, RoboatsCallOperation.OnOrderHandle,
					$"При обработке запроса создания заказа или расчета цены заказа возникло исключение: {ex.Message}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}
		}

		public string ExecuteRequest()
		{
			if(RequestDto.RequestSubType == "price")
			{
				return HandleCalculatePriceRequest();
			}

			if(RequestDto.IsAddOrder == "1")
			{
				return HandleCreateOrderRequest();
			}

			_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.UnknownRequestType, RoboatsCallOperation.OnOrderHandle,
				$"Неизвестный запрос: RequestType={RequestDto.RequestType}, RequestSubType={RequestDto.RequestSubType}. Обратитесь в отдел разработки.");

			return ErrorMessage;
		}

		private string HandleCalculatePriceRequest()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			var counterpartyCount = counterpartyIds.Count();
			if(counterpartyCount > 1)
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.ClientDuplicate, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно рассчитать стоимость заказа. Найдены несколько контрагентов: {string.Join(", ", counterpartyIds)}.");
				return ErrorMessage;
			}

			int counterpartyId;
			if(counterpartyCount == 1)
			{
				counterpartyId = counterpartyIds.First();
			}
			else
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.ClientNotFound, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно рассчитать стоимость заказа. Не найден контрагент.");
				return ErrorMessage;
			}

			if(!AddressId.HasValue)
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.AddressIdNotSpecified, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно рассчитать стоимость заказа. Не заполнен код точки доставки, возможно звонок прерван на выборе адреса. Контрагент {counterpartyId}.");
				return ErrorMessage;
			}

			var deliveryPointIds = _validOrdersProvider.GetLastDeliveryPointIds(ClientPhone, RequestDto.CallGuid, counterpartyId, RoboatsCallFailType.DeliveryPointsNotFound, RoboatsCallOperation.OnOrderHandle);
			if(deliveryPointIds.All(x => x != AddressId))
			{
				//Если точка доставки не найдена у клиента, то значит клиенту предлагались не правильные (не его) точки доставки в запросе ранее.
				return ErrorMessage;
			}

			if(string.IsNullOrWhiteSpace(RequestDto.WaterQuantity))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.WaterNotSpecified, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно рассчитать стоимость заказа. Не указана заказываемая вода, возможно звонок прерван на выборе воды для заказа. Контрагент {counterpartyId}, точка доставки {AddressId}.");
				return ErrorMessage;
			}

			var waters = GetWaters();
			if(!waters.Any())
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.WaterNotSupported, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно рассчитать стоимость заказа. По указанным типам воды ({RequestDto.WaterQuantity}) не удалось найти доступную воду. Контрагент {counterpartyId}, точка доставки {AddressId}. Проверьте справочник типов воды для Roboats.");
				return ErrorMessage;
			}

			if(!int.TryParse(RequestDto.ReturnBottlesCount, out int bottlesReturn))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.BottlesReturnNotFound, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно рассчитать стоимость заказа. Не заполнено количество бутылей на возврат, возможно звонок прерван на вводе бутылей на возврат. Контрагент {counterpartyId}, точка доставки {AddressId}");
				return ErrorMessage;
			}

			return CalculatePrice(counterpartyId, AddressId.Value, waters, bottlesReturn);
		}

		private string HandleCreateOrderRequest()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			var counterpartyCount = counterpartyIds.Count();
			if(counterpartyCount > 1)
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.ClientDuplicate, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно создать заказ. Найдены несколько контрагентов: {string.Join(", ", counterpartyIds)}.");
				return ErrorMessage;
			}

			int counterpartyId;
			if(counterpartyCount == 1)
			{
				counterpartyId = counterpartyIds.First();
			}
			else
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.ClientNotFound, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно создать заказ. Не найден контрагент.");
				return ErrorMessage;
			}

			if(!AddressId.HasValue)
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.AddressIdNotSpecified, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно создать заказ. Не заполнен код точки доставки, возможно звонок прерван на выборе адреса. Контрагент {counterpartyId}.");
				return ErrorMessage;
			}

			var deliveryPointIds = _validOrdersProvider.GetLastDeliveryPointIds(ClientPhone, RequestDto.CallGuid, counterpartyId, RoboatsCallFailType.DeliveryPointsNotFound, RoboatsCallOperation.OnOrderHandle);
			if(deliveryPointIds.All(x => x != AddressId))
			{
				//Если точка доставки не найдена у клиента, то значит клиенту предлагались не правильные (не его) точки доставки в запросе ранее.
				return ErrorMessage;
			}

			if(string.IsNullOrWhiteSpace(RequestDto.WaterQuantity))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.WaterNotSpecified, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно создать заказ. Не указана заказываемая вода, возможно звонок прерван на выборе воды для заказа. Контрагент {counterpartyId}, точка доставки {AddressId}.");
				return ErrorMessage;
			}

			var waters = GetWaters();
			if(!waters.Any())
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.WaterNotSupported, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно создать заказ. По указанным типам воды ({RequestDto.WaterQuantity}) не удалось найти доступную воду. Контрагент {counterpartyId}, точка доставки {AddressId}. Проверьте справочник типов воды для Roboats.");
				return ErrorMessage;
			}

			if(!int.TryParse(RequestDto.ReturnBottlesCount, out int bottlesReturn))
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.BottlesReturnNotFound, RoboatsCallOperation.OnOrderHandle,
					$"Невозможно создать заказ. Не заполнено количество бутылей на возврат, возможно звонок прерван на вводе бутылей на возврат. Контрагент {counterpartyId}, точка доставки {AddressId}.");
				return ErrorMessage;
			}

			return CreateOrderAndGetResult(counterpartyId, AddressId.Value, waters, bottlesReturn);
		}

		private string CalculatePrice(int counterpartyId, int deliveryPointId, IEnumerable<RoboatsWaterInfo> watersInfo, int bottlesReturn)
		{
			var orderArgs = new RoboatsOrderArgs();
			orderArgs.CounterpartyId = counterpartyId;
			orderArgs.DeliveryPointId = deliveryPointId;
			orderArgs.WatersInfo = watersInfo;
			orderArgs.BottlesReturn = bottlesReturn;

			var price = _orderService.GetOrderPrice(orderArgs);
			if(price <= 0)
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.NegativeOrderSum, RoboatsCallOperation.CalculateOrderPrice,
					$"При расчете стоимости заказа получена отрицательная сумма. Вода: {RequestDto.WaterQuantity}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}

			var result = (int)Math.Ceiling(price);

			return $"{result}";
		}

		private string CreateOrderAndGetResult(int counterpartyId, int deliveryPointId, IEnumerable<RoboatsWaterInfo> watersInfo, int bottlesReturn)
		{

			if(!DateTime.TryParseExact(RequestDto.Date, "yyyy-MM-dd", new DateTimeFormatInfo(), DateTimeStyles.None, out DateTime date))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.IncorrectOrderDate, RoboatsCallOperation.CreateOrder,
					$"Невозможно создать заказ. Не заполнена или заполнена некорректно дата доставки (Дата: {RequestDto.Time}), возможно звонок прерван на выборе интервала доставки. Контрагент {counterpartyId}, точка доставки {deliveryPointId}.");
				return ErrorMessage;
			}

			if(!int.TryParse(RequestDto.Time, out int timeId))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.IncorrectOrderInterval, RoboatsCallOperation.CreateOrder,
					$"Невозможно создать заказ. Не заполнен или заполнен некорректно код интервала доставки (Код: {RequestDto.Time}), возможно звонок прерван на выборе интервала доставки. Контрагент {counterpartyId}, точка доставки {deliveryPointId}.");
				return ErrorMessage;
			}

			var deliverySchedule = _roboatsRepository.GetDeliverySchedule(timeId);
			if(deliverySchedule == null)
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.OrderIntervalNotFound, RoboatsCallOperation.CreateOrder,
					$"Невозможно создать заказ. Не найден указанный интервал доставки (Код интервала: {timeId}). Проверьте справочник интервалов доставки для Roboats.");
				return ErrorMessage;
			}

			var isFullOrder = RequestDto.IsFullOrder == "1";

			RoboAtsOrderPayment payment = RoboAtsOrderPayment.Cash;
			if(isFullOrder)
			{
				switch(RequestDto.PaymentType)
				{
					case "terminal":
						payment = RoboAtsOrderPayment.Terminal;
						break;
					case "cash":
						payment = RoboAtsOrderPayment.Cash;
						break;
					case "qrcode":
						payment = RoboAtsOrderPayment.QrCode;
						break;
					default:
						_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.UnknownIsTerminalValue, RoboatsCallOperation.CreateOrder,
							$"Невозможно создать заказ. Не известный тип оплаты. Тип оплаты: {RequestDto.PaymentType}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}. Обратитесь в отдел разработки.");
						return ErrorMessage;
				}
			}
			

			if(!int.TryParse(RequestDto.BanknoteForReturn, out int banknoteForReturn))
			{
				banknoteForReturn = 0;
			}

			var maxBanknoteForReturn = _roboatsSettings.MaxBanknoteForReturn;
			if((banknoteForReturn > maxBanknoteForReturn || banknoteForReturn < 0) && isFullOrder && payment == RoboAtsOrderPayment.Cash)
			{
				//Если "сдача с" выходит за установленные лимиты, то это значит что робот дает клиенту не корректный выбор, надо корректировать лимиты либо исправлять робота Roboats
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.UnknownIsTerminalValue, RoboatsCallOperation.CreateOrder,
					$"Невозможно создать заказ. Указанная \"сдача с\", должна быть меньше лимита ({maxBanknoteForReturn}). Сдача с: {RequestDto.BanknoteForReturn}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}. Обратитесь в отдел разработки.");
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
			orderArgs.BanknoteForReturn = banknoteForReturn;

			try
			{
				if(payment == RoboAtsOrderPayment.QrCode)
				{
					var needAcceptOrder = isFullOrder;
					var task = CreateOrderWithPaymentByQrCode(orderArgs, needAcceptOrder);
					task.Wait();
					return "1";
				}

				if(isFullOrder)
				{
					CreateAndAcceptOrder(orderArgs);
				}
				else
				{
					CreateIncompleteOrder(orderArgs);
				}

				return "1";
			}
			catch(Exception ex)
			{
				if(ex is AggregateException aggregateException && aggregateException.InnerException != null)
				{
					ex = aggregateException.InnerException;
				}
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.Exception, RoboatsCallOperation.CreateOrder,
					$"Произошла ошибка при создании заказа. Ошибка: {ex.Message}. Контрагент {counterpartyId}, точка доставки {deliveryPointId}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}
		}

		private void CreateAndAcceptOrder(RoboatsOrderArgs orderArgs)
		{
			var orderId = _orderService.CreateAndAcceptOrder(orderArgs);
			_callRegistrator.RegisterSuccess(ClientPhone, RequestDto.CallGuid, $"Звонок был успешно завершен. Cоздан и подтвержден заказ {orderId}");
		}

		private void CreateIncompleteOrder(RoboatsOrderArgs orderArgs)
		{
			var orderId = _orderService.CreateIncompleteOrder(orderArgs);
			_callRegistrator.RegisterAborted(ClientPhone, RequestDto.CallGuid, RoboatsCallOperation.CreateOrder, $"Звонок не был успешно завершен. Был создан черновой заказ {orderId}");
		}

		private async Task CreateOrderWithPaymentByQrCode(RoboatsOrderArgs orderArgs, bool needAcceptOrder)
		{
			var order = await _orderService.CreateOrderWithPaymentByQrCode(ClientPhone, orderArgs, needAcceptOrder);
			if(order.OrderStatus == OrderStatus.NewOrder)
			{
				_callRegistrator.RegisterAborted(ClientPhone, RequestDto.CallGuid, RoboatsCallOperation.CreateOrder, $"Был создан черновой заказ {order.Id} с оплатой по QR коду." +
					$" Оплату не удалось сформировать и отправить автоматически." +
					$" Повторите оплату в ручном режиме или свяжитесь с клиентом для выбора другого способа оплаты.");
			}
			else if(order.OrderStatus == OrderStatus.Accepted)
			{
				_callRegistrator.RegisterSuccess(ClientPhone, RequestDto.CallGuid, $"Был подтвержден заказ {order.Id} с оплатой по QR коду." +
					$" Оплата сформирована и отправлена автоматически.");
			}
			else
			{
				_callRegistrator.RegisterAborted(ClientPhone, RequestDto.CallGuid, RoboatsCallOperation.CreateOrder, $"Был создан заказ в некорректном статусе {order.OrderStatus}." +
					$" Обратитесь в отдел разработки.");
			}
		}

		private IEnumerable<RoboatsWaterInfo> GetWaters()
		{
			if(string.IsNullOrWhiteSpace(RequestDto.WaterQuantity))
			{
				return Enumerable.Empty<RoboatsWaterInfo>();
			}

			var waterNodes = RequestDto.WaterQuantity.Split('|').Where(x => !string.IsNullOrWhiteSpace(x));
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
