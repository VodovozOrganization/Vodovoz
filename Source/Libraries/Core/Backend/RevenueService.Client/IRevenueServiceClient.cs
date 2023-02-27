using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RevenueService.Client.Dto;

namespace RevenueService.Client
{
	public interface IRevenueServiceClient
	{
		Task<IList<CounterpartyRevenueServiceDto>> GetCounterpartyInfoAsync(DadataRequestDto dadataRequest, CancellationToken cancellationToken);
	}
}
