using System;
using System.Linq;
using System.Text.RegularExpressions;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using VodovozBusiness.Services.Clients.DeliveryPoints;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Сопоставление адреса из запроса с точкой доставки
	/// </summary>
	public class AddressMatcher : IAddressMatcher
	{
		private readonly IDeliveryPointBuildingNumberParser _buildingNumberParser;

		public AddressMatcher(IDeliveryPointBuildingNumberParser buildingNumberParser)
		{
			_buildingNumberParser = buildingNumberParser
				?? throw new ArgumentNullException(nameof(buildingNumberParser));
		}

		/// <inheritdoc />
		public bool MatchesStreet(string requestStreet, string street, string streetType)
		{
			var requestedStreet = AddressNormalizer.NormalizeStreet(requestStreet);
			var actualStreet = AddressNormalizer.NormalizeStreet(
				string.Join(
					" ",
					new[] { streetType, street }.Where(x => !string.IsNullOrWhiteSpace(x))));

			return !string.IsNullOrWhiteSpace(requestedStreet)
				&& string.Equals(requestedStreet, actualStreet, StringComparison.Ordinal);
		}

		/// <inheritdoc />
		public string GetBuildingNumber(CounterpartyAddressSearchDto request)
		{
			return ParseBuildingDetails(
				request.Building,
				request.Corpus,
				request.Structure,
				request.Letter)
				.BuildingNumber;
		}

		/// <inheritdoc />
		public bool MatchesBuilding(CounterpartyAddressSearchDto request, string building, string letter)
		{
			var requestedDetails = ParseBuildingDetails(
				request.Building,
				request.Corpus,
				request.Structure,
				request.Letter);

			var actualDetails = ParseBuildingDetails(building, null, null, letter);

			if(string.IsNullOrWhiteSpace(requestedDetails.BuildingNumber))
			{
				return string.Equals(
					AddressNormalizer.NormalizeForComparison(request.Building),
					AddressNormalizer.NormalizeForComparison(building),
					StringComparison.Ordinal);
			}

			return string.Equals(
					AddressNormalizer.NormalizePart(requestedDetails.BuildingNumber),
					AddressNormalizer.NormalizePart(actualDetails.BuildingNumber),
					StringComparison.Ordinal)
				&& string.Equals(
					AddressNormalizer.NormalizePart(requestedDetails.Corpus),
					AddressNormalizer.NormalizePart(actualDetails.Corpus),
					StringComparison.Ordinal)
				&& string.Equals(
					AddressNormalizer.NormalizePart(requestedDetails.Structure),
					AddressNormalizer.NormalizePart(actualDetails.Structure),
					StringComparison.Ordinal)
				&& string.Equals(
					AddressNormalizer.NormalizePart(requestedDetails.Liter),
					AddressNormalizer.NormalizePart(actualDetails.Liter),
					StringComparison.Ordinal);
		}

		/// <inheritdoc />
		public bool MatchesRoom(
			CounterpartyAddressSearchDto request,
			string room,
			RoomType roomType,
			string placement)
		{
			var requestedRoomValue = !string.IsNullOrWhiteSpace(request.Room)
				? request.Room
				: request.Placement;

			var requestedRoom = AddressNormalizer.NormalizeRoom(requestedRoomValue);

			if(string.IsNullOrWhiteSpace(requestedRoom))
			{
				return false;
			}

			var requestedRoomType = AddressNormalizer.ParseRoomType(requestedRoomValue);
			var actualRoom = AddressNormalizer.NormalizeRoom(room);

			if(!string.IsNullOrWhiteSpace(actualRoom)
				&& string.Equals(actualRoom, requestedRoom, StringComparison.Ordinal))
			{
				return !requestedRoomType.HasValue || requestedRoomType.Value == roomType;
			}

			var actualPlacement = AddressNormalizer.NormalizeRoom(placement);

			return !string.IsNullOrWhiteSpace(actualPlacement)
				&& string.Equals(actualPlacement, requestedRoom, StringComparison.Ordinal);
		}

		private BuildingNumberDetails ParseBuildingDetails(
			string building,
			string corpus,
			string structure,
			string letter)
		{
			var preparedBuilding = building?.Trim() ?? string.Empty;

			preparedBuilding = Regex.Replace(
				preparedBuilding,
				@"^\s*(д\.?|дом)\s*",
				string.Empty,
				RegexOptions.IgnoreCase);

			preparedBuilding = Regex.Replace(
				preparedBuilding,
				@"\b(корпус|корп)\b",
				"к",
				RegexOptions.IgnoreCase);

			preparedBuilding = Regex.Replace(
				preparedBuilding,
				@"\b(строение|стр)\b",
				"с",
				RegexOptions.IgnoreCase);

			preparedBuilding = Regex.Replace(
				preparedBuilding,
				@"\bК\b(?=\s*\d)",
				"к");

			preparedBuilding = Regex.Replace(
				preparedBuilding,
				@"\bС\b(?=\s*\d)",
				"с");

			preparedBuilding = Regex.Replace(
				preparedBuilding,
				@"\bлит(ера)?\.?\s*",
				string.Empty,
				RegexOptions.IgnoreCase);

			if(string.IsNullOrWhiteSpace(preparedBuilding))
			{
				return new BuildingNumberDetails();
			}

			var details = _buildingNumberParser.ParseBuildingNumber(preparedBuilding);

			if(!string.IsNullOrWhiteSpace(corpus))
			{
				details.Corpus = AddressNormalizer.NormalizePart(corpus);
			}

			if(!string.IsNullOrWhiteSpace(structure))
			{
				details.Structure = AddressNormalizer.NormalizePart(structure);
			}

			if(!string.IsNullOrWhiteSpace(letter))
			{
				details.Liter = AddressNormalizer.NormalizePart(letter);
			}

			return details;
		}
	}
}
