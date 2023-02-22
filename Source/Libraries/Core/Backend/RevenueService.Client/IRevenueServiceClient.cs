using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RevenueService.Client.Dto;

namespace RevenueService.Client
{
	public interface IRevenueServiceClient
	{
		Task<IList<CounterpartyDto>> GetCounterpartyInfoAsync(DadataRequestDto dadataRequest, CancellationToken cancellationToken);
	}
}
