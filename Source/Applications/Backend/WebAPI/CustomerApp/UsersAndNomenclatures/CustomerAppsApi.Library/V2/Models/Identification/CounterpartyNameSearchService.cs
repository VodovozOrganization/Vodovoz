using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Поиск контрагента по ФИО
	/// </summary>
	public class CounterpartyNameSearchService : ICounterpartyNameSearchService
	{
		private readonly ILogger<CounterpartyNameSearchService> _logger;
		private readonly ICounterpartyIdentificationQueries _queries;
		private readonly ICounterpartySearchResultBuilder _resultBuilder;

		public CounterpartyNameSearchService(
			ILogger<CounterpartyNameSearchService> logger,
			ICounterpartyIdentificationQueries queries,
			ICounterpartySearchResultBuilder resultBuilder)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_queries = queries ?? throw new ArgumentNullException(nameof(queries));
			_resultBuilder = resultBuilder ?? throw new ArgumentNullException(nameof(resultBuilder));
		}

		/// <inheritdoc />
		public async Task<CounterpartySearchResultDto> SearchAsync(
			CounterpartyNameSearchDto request,
			CancellationToken cancellationToken)
		{
			var normalizedFullName = PersonNameNormalizer.NormalizePart(request?.FullName);
			var (surname, firstName, patronymic) = PersonNameNormalizer.Parse(request);

			if(string.IsNullOrWhiteSpace(surname)
				&& string.IsNullOrWhiteSpace(firstName)
				&& string.IsNullOrWhiteSpace(patronymic))
			{
				return _resultBuilder.CreateErrorResult("Не указано ФИО");
			}

			_logger.LogInformation("Поиск контрагента по ФИО");

			var counterpartyIds = new HashSet<int>();

			AddCounterpartyIds(
				counterpartyIds,
				await _queries.GetCounterpartyIdsByPersonNameAsync(
					surname,
					firstName,
					patronymic,
					cancellationToken));

			// Для юрлиц ФИО может быть названием организации
			var fullName = !string.IsNullOrWhiteSpace(normalizedFullName)
				? normalizedFullName
				: PersonNameNormalizer.BuildFullName(surname, firstName, patronymic);

			if(!string.IsNullOrWhiteSpace(fullName))
			{
				AddCounterpartyIds(
					counterpartyIds,
					await _queries.GetCounterpartyIdsByLegalNameAsync(fullName, cancellationToken));
			}

			_logger.LogInformation("Поиск контрагента по ФИО: найдено {Count}", counterpartyIds.Count);

			return await _resultBuilder.BuildAsync(counterpartyIds, cancellationToken);
		}

		private static void AddCounterpartyIds(
			ISet<int> counterpartyIds,
			IEnumerable<int> ids)
		{
			foreach(var counterpartyId in ids)
			{
				counterpartyIds.Add(counterpartyId);
			}
		}
	}
}
