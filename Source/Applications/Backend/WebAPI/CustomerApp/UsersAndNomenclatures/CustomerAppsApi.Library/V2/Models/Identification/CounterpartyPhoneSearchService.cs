using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Поиск контрагента по телефону
	/// </summary>
	public class CounterpartyPhoneSearchService : ICounterpartyPhoneSearchService
	{
		private readonly ILogger<CounterpartyPhoneSearchService> _logger;
		private readonly ICounterpartyIdentificationQueries _queries;
		private readonly ICounterpartySearchResultBuilder _resultBuilder;

		public CounterpartyPhoneSearchService(
			ILogger<CounterpartyPhoneSearchService> logger,
			ICounterpartyIdentificationQueries queries,
			ICounterpartySearchResultBuilder resultBuilder)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_queries = queries ?? throw new ArgumentNullException(nameof(queries));
			_resultBuilder = resultBuilder ?? throw new ArgumentNullException(nameof(resultBuilder));
		}

		/// <inheritdoc />
		public async Task<CounterpartySearchResultDto> SearchAsync(
			CounterpartyPhoneSearchDto request,
			CancellationToken cancellationToken)
		{
			var phoneDigits = PhoneNumberNormalizer.Normalize(request?.PhoneNumber);

			if(string.IsNullOrWhiteSpace(phoneDigits) || phoneDigits.Length < 4)
			{
				return _resultBuilder.CreateErrorResult("Не указан или имеет неверный формат номер телефона");
			}

			_logger.LogInformation(
				"Поиск контрагента по телефону {PhoneDigits}",
				PhoneNumberNormalizer.MaskForLog(phoneDigits));

			var counterpartyIds = new HashSet<int>();

			if(phoneDigits.Length >= 10)
			{
				var normalizedPhone = phoneDigits.Substring(phoneDigits.Length - 10);

				AddCounterpartyIds(
					counterpartyIds,
					await _queries.GetCounterpartyIdsByCounterpartyPhoneAsync(
						normalizedPhone,
						false,
						cancellationToken));

				AddCounterpartyIds(
					counterpartyIds,
					await _queries.GetCounterpartyIdsByDeliveryPointPhoneAsync(
						normalizedPhone,
						false,
						cancellationToken));
			}
			else
			{
				// Городские номера могут храниться с кодом города, поэтому ищем по окончанию номера
				AddCounterpartyIds(
					counterpartyIds,
					await _queries.GetCounterpartyIdsByCounterpartyPhoneAsync(
						phoneDigits,
						true,
						cancellationToken));

				AddCounterpartyIds(
					counterpartyIds,
					await _queries.GetCounterpartyIdsByDeliveryPointPhoneAsync(
						phoneDigits,
						true,
						cancellationToken));
			}

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
