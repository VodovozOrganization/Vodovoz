using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Запросы к БД для идентификации контрагента
	/// </summary>
	public interface ICounterpartyIdentificationQueries
	{
		Task<List<int>> GetCounterpartyIdsByCounterpartyPhoneAsync(
			string phone,
			bool useSuffix,
			CancellationToken cancellationToken);
		Task<List<int>> GetCounterpartyIdsByDeliveryPointPhoneAsync(
			string phone,
			bool useSuffix,
			CancellationToken cancellationToken);
		Task<List<int>> GetCounterpartyIdsByPersonNameAsync(
			string surname,
			string firstName,
			string patronymic,
			CancellationToken cancellationToken);
		Task<List<int>> GetCounterpartyIdsByLegalNameAsync(
			string legalName,
			CancellationToken cancellationToken);
		Task<List<CounterpartyAddressCandidate>> GetAddressCandidatesAsync(
			string street,
			string city,
			string buildingNumber,
			CancellationToken cancellationToken);
		Task<List<CounterpartySummary>> GetCounterpartiesByIdsAsync(
			IReadOnlyCollection<int> counterpartyIds,
			CancellationToken cancellationToken);
		Task<List<CounterpartyPhoneInfo>> GetPhonesAsync(
			IReadOnlyCollection<int> counterpartyIds,
			CancellationToken cancellationToken);
		Task<List<CounterpartyDeliveryPointInfo>> GetDeliveryPointsAsync(
			IReadOnlyCollection<int> counterpartyIds,
			CancellationToken cancellationToken);
		Task<List<CounterpartyOrderInfo>> GetOrdersAsync(
			int counterpartyId,
			CancellationToken cancellationToken);
		Task<bool> CounterpartyExistsAsync(
			int counterpartyId,
			CancellationToken cancellationToken);
	}
}
