using Dadata.Model;
using RevenueService.Client.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace RevenueService.Client
{
	public interface IRevenueServiceClient
	{
		Task<RevenueServiceResponseDto> GetCounterpartyInfoAsync(DadataRequestDto dadataRequest, CancellationToken cancellationToken);
		Task<PartyStatus> GetCounterpartyStatus(string inn, string kpp, CancellationToken cancellationToken);
	}
}
