using Microsoft.Extensions.Logging;
using RoboatsService.Monitoring;
using RoboatsService.OrderValidation;
using RoboAtsService.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;

namespace RoboatsService.Handlers
{
	/// <summary>
	/// Обработчик запросов данных о заказе
	/// </summary>
	public class LastOrderHandler : GetRequestHandlerBase
	{
		private readonly ILogger<LastOrderHandler> _logger;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly RoboatsCallRegistrator _callRegistrator;
		private readonly ValidOrdersProvider _validOrdersProvider;

		public LastOrderRequestType RequestType { get; }

		public override string Request => RoboatsRequestType.LastOrder;

		public LastOrderHandler(ILogger<LastOrderHandler> logger, IRoboatsRepository roboatsRepository, RequestDto requestDto, RoboatsCallRegistrator callRegistrator, ValidOrdersProvider validOrdersProvider) : base(requestDto)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_callRegistrator = callRegistrator ?? throw new ArgumentNullException(nameof(callRegistrator));
			_validOrdersProvider = validOrdersProvider ?? throw new ArgumentNullException(nameof(validOrdersProvider));
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
			try
			{
				return ExecuteRequest();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При обработке запроса информации о последнем заказе возникло исключение");
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.Exception, RoboatsCallOperation.GetLastOrderCheck,
						$"При обработке запроса информации о последнем заказе возникло исключение: {ex.Message}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}
		}

		public string ExecuteRequest()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			var counterpartyCount = counterpartyIds.Count();
			if(counterpartyCount > 1)
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.ClientDuplicate, RoboatsCallOperation.GetLastOrderCheck,
					$"Для телефона {ClientPhone} найдены несколько контрагентов: {string.Join(", ", counterpartyIds)}.");
				return ErrorMessage;
			}

			int counterpartyId;
			if(counterpartyCount == 1)
			{
				counterpartyId = counterpartyIds.First();
			}
			else
			{
				_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.ClientNotFound, RoboatsCallOperation.GetLastOrderCheck,
					$"Не найден контрагент.");
				return ErrorMessage;
			}

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
					_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.UnknownRequestType, RoboatsCallOperation.GetLastOrderCheck,
						$"Неизвестный тип запроса: {RequestType}. Обратитесь в отдел разработки.");
					return ErrorMessage;
			}
		}

		private string GetLastOrderCheck(int counterpartyId)
		{
			var deliveryPointIds = _validOrdersProvider.GetLastDeliveryPointIds(ClientPhone, RequestDto.CallGuid, counterpartyId, RoboatsCallFailType.OrderNotFound, RoboatsCallOperation.GetLastOrderCheck);
			if(deliveryPointIds.Any())
			{
				return "1";
			}
			else
			{
				return "0";
			}
		}

		private string GetLastOrderId(int counterpartyId)
		{
			if(!AddressId.HasValue)
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.AddressIdNotSpecified, RoboatsCallOperation.GetLastOrderId,
					$"В запросе не указан код точки доставки, возможно звонок прерван в момент выбора точки доставки.");
				return ErrorMessage;
			}

			var order = _validOrdersProvider.GetLastOrder(ClientPhone, RequestDto.CallGuid, counterpartyId, AddressId.Value, RoboatsCallFailType.OrderNotFound, RoboatsCallOperation.GetLastOrderId);
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
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.IncorrectOrderId, RoboatsCallOperation.GetWaterInfo,
					$"Некорректный номер заказа (Номер заказа: {RequestDto.OrderId}). Контрагент: {counterpartyId}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}

			var availableWaters = _roboatsRepository.GetWaterTypes();
			if(!availableWaters.Any())
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.AvailableWatersNotFound, RoboatsCallOperation.GetWaterInfo,
					$"Не найдены доступные для заказа типы воды в справочнике. Проверьте справочник типов воды для Roboats.");
				return ErrorMessage;
			}

			var waterQuantity = _roboatsRepository.GetWatersQuantityFromOrder(counterpartyId, orderId);
			if(!waterQuantity.Any())
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.WaterInOrderNotFound, RoboatsCallOperation.GetWaterInfo,
					$"Не найдена вода в заказе {orderId}");
				return ErrorMessage;
			}

			List<string> results = new List<string>();
			foreach(var waterItem in waterQuantity)
			{
				var roboatsWaterInfo = availableWaters.FirstOrDefault(x => x.Nomenclature.Id == waterItem.NomenclatureId);
				if(roboatsWaterInfo == null)
				{
					_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.WaterNotSupported, RoboatsCallOperation.GetWaterInfo,
						$"Вода Id {waterItem.NomenclatureId} в заказе {orderId} не доступна для заказа (не найдена в справочнике воды для Roboats)");
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
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.IncorrectOrderId, RoboatsCallOperation.GetBottlesReturn,
					$"Некорректный номер заказа (Номер заказа: {RequestDto.OrderId}). Обратитесь в отдел разработки.");
				return ErrorMessage;
			}

			var bottlesReturn = _roboatsRepository.GetBottlesReturnForOrder(counterpartyId, orderId);
			if(!bottlesReturn.HasValue)
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.BottlesReturnNotFound, RoboatsCallOperation.GetBottlesReturn,
					$"Не найдено количество возвратной тары для заказа {orderId}");
				return "0";
			}

			return $"{bottlesReturn.Value}";
		}
	}
}
