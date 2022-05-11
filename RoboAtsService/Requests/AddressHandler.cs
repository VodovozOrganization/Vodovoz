using System;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.EntityRepositories.Counterparties;

namespace RoboAtsService.Requests
{
	/// <summary>
	/// Обработчик запросов получения данных об адресе
	/// </summary>
	public class AddressHandler : GetRequestHandlerBase
	{
		private readonly RoboatsRepository _roboatsRepository;

		public override string Request => RoboatsRequestType.Address;

		public AddressHandler(RoboatsRepository roboatsRepository, RequestDto requestDto) : base(requestDto)
		{
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
		}

		public override string Execute()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			if(counterpartyIds.Count() != 1)
			{
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
			var deliveryPointIds = _roboatsRepository.GetLastDeliveryPointIds(counterpartyId);

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
			if(!int.TryParse(RequestDto.AddressId, out int addressId)){
				return ErrorMessage;
			}

			var streetId = _roboatsRepository.GetRoboAtsStreetId(counterpartyId, addressId);

			if(streetId.HasValue)
			{
				return $"{streetId.Value}";
			}
			else
			{
				return "NO DATA";
			}
		}

		private string GetAddressHouseNumber(int counterpartyId)
		{
			if(!int.TryParse(RequestDto.AddressId, out int addressId))
			{
				return ErrorMessage;
			}

			var deliveryPointBuilding = _roboatsRepository.GetDeliveryPointBuilding(addressId, counterpartyId);
			if(string.IsNullOrWhiteSpace(deliveryPointBuilding))
			{
				return "NO DATA";
			}

			var result = GetHouseNumber(deliveryPointBuilding);
			if(string.IsNullOrWhiteSpace(result))
			{
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
				return ErrorMessage;
			}

			var deliveryPointBuilding = _roboatsRepository.GetDeliveryPointBuilding(addressId, counterpartyId);
			if(string.IsNullOrWhiteSpace(deliveryPointBuilding))
			{
				return "NO DATA";
			}

			var result = GetCorpusNumber(deliveryPointBuilding);
			if(string.IsNullOrWhiteSpace(result))
			{
				return "NO DATA";
			}

			return result;
		}

		private string GetCorpusNumber(string fullBuildingNumber)
		{
			Regex regex = new Regex(@"((к[ ]*[.]?[ ]*\d+)|(кор[ ]*[.]?[ ]*\d+)|(корпус[ ]*[.]?[ ]*\d+)){1,}");
			var match = regex.Match(fullBuildingNumber);

			Regex regexDigits = new Regex(@"\d{1,}");
			var corpusNumber = regexDigits.Match(match.Value);
			return corpusNumber.Value;
		}

		private string GetAddressApartmentNumber(int counterpartyId)
		{
			if(!int.TryParse(RequestDto.AddressId, out int addressId))
			{
				return ErrorMessage;
			}


			var deliveryPointApartment = _roboatsRepository.GetDeliveryPointApartment(addressId, counterpartyId);
			if(string.IsNullOrWhiteSpace(deliveryPointApartment))
			{
				return "NO DATA";
			}

			var result = GetApartmentNumber(deliveryPointApartment);
			if(string.IsNullOrWhiteSpace(result))
			{
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
