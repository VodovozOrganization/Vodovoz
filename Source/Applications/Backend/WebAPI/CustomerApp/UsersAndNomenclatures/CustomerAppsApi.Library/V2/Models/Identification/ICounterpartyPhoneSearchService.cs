using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Поиск контрагента по телефону
	/// </summary>
	public interface ICounterpartyPhoneSearchService
	{
		Task<CounterpartySearchResultDto> SearchAsync(
			CounterpartyPhoneSearchDto request,
			CancellationToken cancellationToken);
	}
}
