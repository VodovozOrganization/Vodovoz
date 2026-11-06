using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Поиск контрагента по ФИО
	/// </summary>
	public interface ICounterpartyNameSearchService
	{
		Task<CounterpartySearchResultDto> SearchAsync(
			CounterpartyNameSearchDto request,
			CancellationToken cancellationToken);
	}
}
