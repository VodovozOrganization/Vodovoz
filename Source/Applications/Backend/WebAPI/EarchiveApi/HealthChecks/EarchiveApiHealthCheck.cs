using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace EarchiveApi.HealthChecks
{
	public class EarchiveApiHealthCheck : VodovozHealthCheckBase
	{
		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var serviceAddress = $"https://localhost:7101/";

			var grpcChannelOptions = new GrpcChannelOptions
			{
				HttpHandler = new GrpcWebHandler(new HttpClientHandler())
			};

			var channel = GrpcChannel.ForAddress(serviceAddress, grpcChannelOptions);

			var earchiveUpdClient = new EarchiveApiTestClient.EarchiveUpd.EarchiveUpdClient(channel);

			var response = earchiveUpdClient.GetAddresses(new EarchiveApiTestClient.CounterpartyInfo { Id = 2 });

			await response.ResponseStream.MoveNext(CancellationToken.None);

			var address = response.ResponseStream.Current;

			return new VodovozHealthResultDto { IsHealthy = address != null };
		}
	}
}
