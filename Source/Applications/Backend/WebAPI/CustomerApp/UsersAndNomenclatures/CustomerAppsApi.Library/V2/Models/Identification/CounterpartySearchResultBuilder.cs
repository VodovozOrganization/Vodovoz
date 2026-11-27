using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Сборка результата поиска контрагента
	/// </summary>
	public class CounterpartySearchResultBuilder : ICounterpartySearchResultBuilder
	{
		private readonly ICounterpartyIdentificationQueries _queries;

		public CounterpartySearchResultBuilder(ICounterpartyIdentificationQueries queries)
		{
			_queries = queries ?? throw new ArgumentNullException(nameof(queries));
		}

		/// <inheritdoc />
		public async Task<CounterpartySearchResultDto> BuildAsync(
			ICollection<int> counterpartyIds,
			CancellationToken cancellationToken)
		{
			if(counterpartyIds.Count == 0)
			{
				return CreateNotFoundResult();
			}

			var ids = counterpartyIds
				.Distinct()
				.Take(CounterpartyIdentificationOptions.MaxSearchResults)
				.ToList();

			var counterparties = await _queries.GetCounterpartiesByIdsAsync(ids, cancellationToken);

			if(counterparties.Count == 0)
			{
				return CreateNotFoundResult();
			}

			var phones = await _queries.GetPhonesAsync(ids, cancellationToken);
			var deliveryPoints = await _queries.GetDeliveryPointsAsync(ids, cancellationToken);

			var phoneMap = BuildPhoneMap(phones);
			var stopListMap = counterparties.ToDictionary(x => x.Id, x => x.IsDeliveriesClosed);
			var deliveryPointMap = BuildDeliveryPointMap(deliveryPoints, stopListMap);

			var result = counterparties
				.OrderBy(c => c.Name)
				.Select(counterparty => CreateCounterpartyDto(
					counterparty.Id,
					counterparty.Name,
					counterparty.FullName,
					counterparty.Surname,
					counterparty.FirstName,
					counterparty.Patronymic,
					counterparty.PersonType,
					counterparty.IsArchive,
					counterparty.IsDeliveriesClosed,
					phoneMap,
					deliveryPointMap.TryGetValue(counterparty.Id, out var points)
						? points
						: new List<CounterpartyAddressDto>()))
				.ToList();

			return new CounterpartySearchResultDto
			{
				Status = result.Count == 1
					? CounterpartySearchStatus.Found
					: CounterpartySearchStatus.MultipleFound,
				Counterparties = result
			};
		}

		/// <inheritdoc />
		public async Task<CounterpartySearchResultDto> BuildForAddressAsync(
			IReadOnlyCollection<CounterpartyAddressCandidate> matchedDeliveryPoints,
			CancellationToken cancellationToken)
		{
			if(matchedDeliveryPoints.Count == 0)
			{
				return CreateNotFoundResult();
			}

			var counterpartyGroups = matchedDeliveryPoints
				.GroupBy(c => c.CounterpartyId)
				.Take(CounterpartyIdentificationOptions.MaxSearchResults)
				.ToList();

			var counterpartyIds = counterpartyGroups.Select(g => g.Key).ToList();
			var phones = await _queries.GetPhonesAsync(counterpartyIds, cancellationToken);
			var phoneMap = BuildPhoneMap(phones);

			var result = new List<CounterpartyShortDto>();

			foreach(var counterpartyGroup in counterpartyGroups)
			{
				var counterparty = counterpartyGroup.First();

				var deliveryPoints = counterpartyGroup
					.GroupBy(c => c.Id)
					.Select(g => g.First())
					.Take(CounterpartyIdentificationOptions.MaxDeliveryPointsPerCounterparty)
					.Select(c => new CounterpartyAddressDto
					{
						ErpDeliveryPointId = c.Id,
						Address = AddressNormalizer.BuildAddress(
							c.CompiledAddress,
							c.City,
							c.StreetType,
							c.Street,
							c.Building,
							c.Letter,
							c.Room,
							c.RoomType,
							c.Placement),
						IsStopList = counterparty.IsDeliveriesClosed,
						IsActive = c.IsActive
					})
					.ToList();

				result.Add(CreateCounterpartyDto(
					counterparty.CounterpartyId,
					counterparty.Name,
					counterparty.FullName,
					counterparty.Surname,
					counterparty.FirstName,
					counterparty.Patronymic,
					counterparty.PersonType,
					counterparty.IsArchive,
					counterparty.IsDeliveriesClosed,
					phoneMap,
					deliveryPoints));
			}

			return new CounterpartySearchResultDto
			{
				Status = result.Count == 1
					? CounterpartySearchStatus.Found
					: CounterpartySearchStatus.MultipleFound,
				Counterparties = result
			};
		}

		/// <inheritdoc />
		public CounterpartySearchResultDto CreateErrorResult(string errorDescription)
		{
			return new CounterpartySearchResultDto
			{
				Status = CounterpartySearchStatus.Error,
				ErrorDescription = errorDescription
			};
		}

		private static CounterpartySearchResultDto CreateNotFoundResult()
		{
			return new CounterpartySearchResultDto
			{
				Status = CounterpartySearchStatus.NotFound
			};
		}

		private static Dictionary<int, List<string>> BuildPhoneMap(
			IReadOnlyCollection<CounterpartyPhoneInfo> phones)
		{
			var result = new Dictionary<int, List<string>>();

			foreach(var phone in phones)
			{
				var value = string.IsNullOrWhiteSpace(phone.Number)
					? phone.DigitsNumber
					: phone.Number;

				if(string.IsNullOrWhiteSpace(value))
				{
					continue;
				}

				if(!result.TryGetValue(phone.CounterpartyId, out var counterpartyPhones))
				{
					counterpartyPhones = new List<string>();
					result[phone.CounterpartyId] = counterpartyPhones;
				}

				if(counterpartyPhones.Count < CounterpartyIdentificationOptions.MaxPhonesPerCounterparty
					&& !counterpartyPhones.Contains(value))
				{
					counterpartyPhones.Add(value);
				}
			}

			return result;
		}

		private static Dictionary<int, List<CounterpartyAddressDto>> BuildDeliveryPointMap(
			IReadOnlyCollection<CounterpartyDeliveryPointInfo> deliveryPoints,
			IReadOnlyDictionary<int, bool> stopListMap)
		{
			var result = new Dictionary<int, List<CounterpartyAddressDto>>();

			foreach(var deliveryPoint in deliveryPoints)
			{
				if(!result.TryGetValue(deliveryPoint.CounterpartyId, out var points))
				{
					points = new List<CounterpartyAddressDto>();
					result[deliveryPoint.CounterpartyId] = points;
				}

				if(points.Count >= CounterpartyIdentificationOptions.MaxDeliveryPointsPerCounterparty)
				{
					continue;
				}

				points.Add(new CounterpartyAddressDto
				{
					ErpDeliveryPointId = deliveryPoint.Id,
					Address = AddressNormalizer.BuildAddress(
						deliveryPoint.CompiledAddress,
						deliveryPoint.City,
						deliveryPoint.StreetType,
						deliveryPoint.Street,
						deliveryPoint.Building,
						deliveryPoint.Letter,
						deliveryPoint.Room,
						deliveryPoint.RoomType,
						deliveryPoint.Placement),
					IsStopList = stopListMap.TryGetValue(deliveryPoint.CounterpartyId, out var isStopList)
						&& isStopList,
					IsActive = deliveryPoint.IsActive
				});
			}

			return result;
		}

		private static CounterpartyShortDto CreateCounterpartyDto(
			int id,
			string name,
			string fullName,
			string surname,
			string firstName,
			string patronymic,
			PersonType personType,
			bool isArchive,
			bool isDeliveriesClosed,
			IReadOnlyDictionary<int, List<string>> phones,
			IReadOnlyCollection<CounterpartyAddressDto> deliveryPoints)
		{
			var displayName = !string.IsNullOrWhiteSpace(fullName)
				? fullName.Trim()
				: name?.Trim();

			if(string.IsNullOrWhiteSpace(displayName) && personType == PersonType.natural)
			{
				displayName = PersonNameNormalizer.BuildFullName(surname, firstName, patronymic).Trim();
			}

			var counterparty = new CounterpartyShortDto
			{
				ErpCounterpartyId = id,
				Name = displayName,
				FullName = fullName?.Trim(),
				Surname = surname?.Trim(),
				FirstName = firstName?.Trim(),
				Patronymic = patronymic?.Trim(),
				PersonType = personType,
				PersonTypeName = GetPersonTypeName(personType),
				IsArchive = isArchive,
				IsDeliveriesClosed = isDeliveriesClosed,
				Phones = phones.TryGetValue(id, out var phoneList)
					? phoneList
					: new List<string>(),
				DeliveryPoints = deliveryPoints.ToList()
			};

			foreach(var deliveryPoint in counterparty.DeliveryPoints)
			{
				deliveryPoint.IsStopList = isDeliveriesClosed;
			}

			return counterparty;
		}

		private static string GetPersonTypeName(PersonType personType)
		{
			return personType == PersonType.legal
				? "Юридическое лицо"
				: "Физическое лицо";
		}
	}
}
