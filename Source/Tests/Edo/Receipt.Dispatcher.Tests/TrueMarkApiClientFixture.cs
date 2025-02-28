using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;
using TrueMarkApi.Client;

namespace Receipt.Dispatcher.Tests
{
	public class TrueMarkApiClientFixture : ITrueMarkApiClient
	{
		public Task<TrueMarkRegistrationResultDto> GetParticipantRegistrationForWaterStatusAsync(string url, string inn, CancellationToken cancellationToken)
		{
			return Task.FromResult(new TrueMarkRegistrationResultDto
			{
				RegistrationStatusString = ""
			});
		}

		public Task<ProductInstancesInfoResponse> GetProductInstanceInfoAsync(IEnumerable<string> identificationCodes, CancellationToken cancellationToken)
		{
			return Task.FromResult(new ProductInstancesInfoResponse
			{
				InstanceStatuses = new List<ProductInstanceStatus>()
			});
		}
	}
}
