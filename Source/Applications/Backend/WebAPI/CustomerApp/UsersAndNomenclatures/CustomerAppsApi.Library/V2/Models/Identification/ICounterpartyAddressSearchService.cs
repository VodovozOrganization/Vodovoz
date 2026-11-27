using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Поиск контрагента по адресу
	/// </summary>
	public interface ICounterpartyAddressSearchService
	{
		Task<CounterpartySearchResultDto> SearchAsync(
			CounterpartyAddressSearchDto request,
			CancellationToken cancellationToken);
	}
}
