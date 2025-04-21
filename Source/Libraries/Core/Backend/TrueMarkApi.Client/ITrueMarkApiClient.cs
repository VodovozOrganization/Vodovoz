using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;

namespace TrueMarkApi.Client
{
	public interface ITrueMarkApiClient
	{
		Task<TrueMarkRegistrationResultDto> GetParticipantRegistrationForWaterStatusAsync(string url, string inn, CancellationToken cancellationToken);
		Task<ProductInstancesInfoResponse> GetProductInstanceInfoAsync(IEnumerable<string> identificationCodes, CancellationToken cancellationToken);
		Task<string> GetCrptTokenAsync(string certificateThumbPrint, string inn, CancellationToken cancellationToken);
	}
}
