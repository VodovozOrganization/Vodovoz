using Microsoft.Extensions.Logging;
using RoboatsService.Monitoring;
using RoboatsService.OrderValidation;
using RoboAtsService.Contracts.Requests;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;

namespace RoboatsService.Handlers
{
	/// <summary>
	/// Обработчик запросов получения данных об адресе
	/// </summary>
	public class AddressHandler : GetRequestHandlerBase
	{
		private readonly ILogger<AddressHandler> _logger;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly RoboatsCallRegistrator _callRegistrator;
		private readonly ValidOrdersProvider _validOrdersProvider;

		public override string Request => RoboatsRequestType.Address;

		public AddressHandler(ILogger<AddressHandler> logger, IRoboatsRepository roboatsRepository, RequestDto requestDto, RoboatsCallRegistrator callRegistrator, ValidOrdersProvider validOrdersProvider) : base(requestDto)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_callRegistrator = callRegistrator ?? throw new ArgumentNullException(nameof(callRegistrator));
			_validOrdersProvider = validOrdersProvider ?? throw new ArgumentNullException(nameof(validOrdersProvider));
		}

		public override string Execute()
		{
			try
			{
				return ExecuteRequest();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При обработке запроса информации об адресе возникло исключение");
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.Exception, RoboatsCallOperation.OnAddressHandle,
						$"При обработке запроса информации об адресе возникло исключение: {ex.Message}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}
		}

		private string ExecuteRequest()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			var counterpartyCount = counterpartyIds.Count();
			if(counterpartyCount != 1)
			{
				if(counterpartyCount > 1)
				{
					_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.ClientDuplicate, RoboatsCallOperation.ClientCheck,
						$"Найдены несколько контрагентов: {string.Join(", ", counterpartyIds)}.");
				}
				else
				{
					_callRegistrator.RegisterTerminatingFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.ClientNotFound, RoboatsCallOperation.ClientCheck,
						$"Не найден контрагент.");
				}

				return ErrorMessage;
			}

			var counterpartyId = counterpartyIds.First();

			switch(RequestDto.RequestSubType)
			{
				case "quantity":
					return GetDeliveryPoints(counterpartyId);
				case "street_code":
					return GetRoboatsStreetId(counterpartyId);
				case "house_number":
					return GetAddressHouseNumber(counterpartyId);
				case "building_number":
					return GetAddressCorpusNumber(counterpartyId);
				case "flat_number":
					return GetAddressApartmentNumber(counterpartyId);
				default:
					return ErrorMessage;
			}
		}

		public string GetDeliveryPoints(int counterpartyId)
		{
			var deliveryPointIds = _validOrdersProvider.GetLastDeliveryPointIds(ClientPhone, RequestDto.CallGuid, counterpartyId, RoboatsCallFailType.DeliveryPointsNotFound, RoboatsCallOperation.GetDeliveryPoints);
			if(deliveryPointIds.Any())
			{
				return string.Join('|', deliveryPointIds);
			}
			else
			{
				return "NO DATA";
			}
		}

		public string GetRoboatsStreetId(int counterpartyId)
		{
			if(!int.TryParse(RequestDto.AddressId, out int addressId))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.IncorrectAddressId, RoboatsCallOperation.GetStreetId,
					$"Некорректный код точки доставки {RequestDto.AddressId}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}

			var streetId = _roboatsRepository.GetRoboAtsStreetId(counterpartyId, addressId);

			if(streetId.HasValue)
			{
				return $"{streetId.Value}";
			}
			else
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.StreetNotFound, RoboatsCallOperation.GetStreetId,
					$"Для контрагента {counterpartyId} по точке доставки {addressId} не найдена улица в справочнике Roboats. Проверьте справочник улиц Roboats.");
				return "NO DATA";
			}
		}

		private string GetAddressHouseNumber(int counterpartyId)
		{
			if(!int.TryParse(RequestDto.AddressId, out int addressId))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.IncorrectAddressId, RoboatsCallOperation.GetHouseNumber,
					$"Некорректный код точки доставки {RequestDto.AddressId}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}

			string result = string.Empty;
			var deliveryPointBuilding = _roboatsRepository.GetDeliveryPointBuilding(addressId, counterpartyId);
			if(!string.IsNullOrWhiteSpace(deliveryPointBuilding))
			{
				result = GetHouseNumber(deliveryPointBuilding);
			}

			if(string.IsNullOrWhiteSpace(deliveryPointBuilding) || string.IsNullOrWhiteSpace(result))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.HouseNotFound, RoboatsCallOperation.GetHouseNumber,
					$"Для контрагента {counterpartyId} по точке доставки {addressId} не найден номер дома.");
				return "NO DATA";
			}

			return result;
		}

		private string GetHouseNumber(string fullBuildingNumber)
		{
			Regex regex = new Regex(@"\d{1,}");
			var match = regex.Match(fullBuildingNumber);
			return match.Value;
		}

		private string GetAddressCorpusNumber(int counterpartyId)
		{
			if(!int.TryParse(RequestDto.AddressId, out int addressId))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.IncorrectAddressId, RoboatsCallOperation.GetCorpusNumber,
					$"Некорректный код точки доставки {RequestDto.AddressId}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}
			string result = string.Empty;
			var deliveryPointBuilding = _roboatsRepository.GetDeliveryPointBuilding(addressId, counterpartyId);

			if(!string.IsNullOrWhiteSpace(deliveryPointBuilding))
			{
				result = GetCorpusNumber(deliveryPointBuilding);
			}

			if(string.IsNullOrWhiteSpace(deliveryPointBuilding))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.HouseNotFound, RoboatsCallOperation.GetCorpusNumber,
					$"Для контрагента {counterpartyId} по точке доставки {addressId} не найден номер дома.");
				return "NO DATA";
			}

			if(string.IsNullOrWhiteSpace(result))
			{
				//решили не писать в мониторинг roboats т.к. есть адреса без корпусов
				return "NO DATA";
			}

			return result;
		}

		private string GetCorpusNumber(string fullBuildingNumber)
		{
			Regex regex = new Regex(@"((к[ ]*[.]?[ ]*\d+)|(кор[ ]*[.]?[ ]*\d+)|(корп[ ]*[.]?[ ]*\d+)|(корпус[ ]*[.]?[ ]*\d+)){1,}"); //на всякий решили добавить поиск по корп
			var match = regex.Match(fullBuildingNumber);

			Regex regexDigits = new Regex(@"\d{1,}");
			var corpusNumber = regexDigits.Match(match.Value);
			return corpusNumber.Value;
		}

		private string GetAddressApartmentNumber(int counterpartyId)
		{
			if(!int.TryParse(RequestDto.AddressId, out int addressId))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.IncorrectAddressId, RoboatsCallOperation.GetStreetId,
					$"Некорректный код точки доставки {RequestDto.AddressId}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}


			string result = string.Empty;
			var deliveryPointApartment = _roboatsRepository.GetDeliveryPointApartment(addressId, counterpartyId);
			if(!string.IsNullOrWhiteSpace(deliveryPointApartment))
			{
				result = GetApartmentNumber(deliveryPointApartment);
			}

			if(string.IsNullOrWhiteSpace(deliveryPointApartment) || string.IsNullOrWhiteSpace(result))
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.ApartmentNotFound, RoboatsCallOperation.GetApartmentNumber,
					$"Для контрагента {counterpartyId} по точке доставки {addressId} не найден номер квартиры.");
				return "NO DATA";
			}

			return result;
		}

		private string GetApartmentNumber(string fullApartmentNumber)
		{
			Regex regex = new Regex(@"\d{1,}");
			var match = regex.Match(fullApartmentNumber);
			return match.Value;
		}
	}

}
