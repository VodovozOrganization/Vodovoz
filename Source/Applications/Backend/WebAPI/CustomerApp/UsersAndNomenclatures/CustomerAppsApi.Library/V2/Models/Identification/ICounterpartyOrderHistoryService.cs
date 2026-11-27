using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Получение истории заказов контрагента
	/// </summary>
	public interface ICounterpartyOrderHistoryService
	{
		Task<CounterpartyOrderHistoryDto> GetAsync(
			CounterpartyOrderHistoryRequestDto request,
			CancellationToken cancellationToken);
	}
}
