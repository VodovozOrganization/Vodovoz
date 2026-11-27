using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Сборка результата поиска контрагента
	/// </summary>
	public interface ICounterpartySearchResultBuilder
	{
		/// <summary>
		/// Собирает результат по идентификаторам найденных контрагентов
		/// </summary>
		Task<CounterpartySearchResultDto> BuildAsync(
			ICollection<int> counterpartyIds,
			CancellationToken cancellationToken);
		/// <summary>
		/// Собирает результат поиска по адресу
		/// </summary>
		Task<CounterpartySearchResultDto> BuildForAddressAsync(
			IReadOnlyCollection<CounterpartyAddressCandidate> matchedDeliveryPoints,
			CancellationToken cancellationToken);
		/// <summary>
		/// Создаёт результат с ошибкой
		/// </summary>
		CounterpartySearchResultDto CreateErrorResult(string errorDescription);
	}
}
