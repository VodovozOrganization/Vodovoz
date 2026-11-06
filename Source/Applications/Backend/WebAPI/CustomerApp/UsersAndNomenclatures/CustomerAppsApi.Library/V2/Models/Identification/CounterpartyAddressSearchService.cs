using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Поиск контрагента по адресу
	/// </summary>
	public class CounterpartyAddressSearchService : ICounterpartyAddressSearchService
	{
		private readonly ILogger<CounterpartyAddressSearchService> _logger;
		private readonly ICounterpartyIdentificationQueries _queries;
		private readonly ICounterpartySearchResultBuilder _resultBuilder;
		private readonly IAddressMatcher _addressMatcher;

		public CounterpartyAddressSearchService(
			ILogger<CounterpartyAddressSearchService> logger,
			ICounterpartyIdentificationQueries queries,
			ICounterpartySearchResultBuilder resultBuilder,
			IAddressMatcher addressMatcher)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_queries = queries ?? throw new ArgumentNullException(nameof(queries));
			_resultBuilder = resultBuilder ?? throw new ArgumentNullException(nameof(resultBuilder));
			_addressMatcher = addressMatcher ?? throw new ArgumentNullException(nameof(addressMatcher));
		}

		/// <inheritdoc />
		public async Task<CounterpartySearchResultDto> SearchAsync(
			CounterpartyAddressSearchDto request,
			CancellationToken cancellationToken)
		{
			if(request == null)
			{
				return _resultBuilder.CreateErrorResult("Не передан адрес для поиска");
			}

			if(string.IsNullOrWhiteSpace(request.Street) || string.IsNullOrWhiteSpace(request.Building))
			{
				return _resultBuilder.CreateErrorResult("Не указаны улица или дом");
			}

			if(string.IsNullOrWhiteSpace(request.Room) && string.IsNullOrWhiteSpace(request.Placement))
			{
				return _resultBuilder.CreateErrorResult("Не указано помещение");
			}

			var street = AddressNormalizer.NormalizeStreet(request.Street);
			var city = AddressNormalizer.NormalizeCity(request.City);

			if(string.IsNullOrWhiteSpace(street))
			{
				return _resultBuilder.CreateErrorResult("Не указана улица");
			}

			_logger.LogInformation(
				"Поиск контрагента по адресу: город {City}, улица {Street}, дом {Building}",
				city,
				street,
				request.Building);

			var buildingNumber = _addressMatcher.GetBuildingNumber(request);

			var candidates = await _queries.GetAddressCandidatesAsync(
				street,
				city,
				buildingNumber,
				cancellationToken);

			var matchedDeliveryPoints = candidates
				.Where(c => _addressMatcher.MatchesStreet(request.Street, c.Street, c.StreetType)
					&& _addressMatcher.MatchesBuilding(request, c.Building, c.Letter)
					&& _addressMatcher.MatchesRoom(request, c.Room, c.RoomType, c.Placement))
				.ToList();

			_logger.LogInformation(
				"Поиск контрагента по адресу: кандидатов {Candidates}, совпало {Matched}",
				candidates.Count,
				matchedDeliveryPoints.Count);

			if(matchedDeliveryPoints.Count == 0 && candidates.Count > 0)
			{
				foreach(var candidate in candidates.Take(5))
				{
					_logger.LogInformation(
						"Кандидат по адресу: id {DeliveryPointId}, город {City}, тип улицы {StreetType}, улица {Street}, дом {Building}",
						candidate.Id,
						candidate.City,
						candidate.StreetType,
						candidate.Street,
						candidate.Building);
				}
			}

			return await _resultBuilder.BuildForAddressAsync(matchedDeliveryPoints, cancellationToken);
		}
	}
}
