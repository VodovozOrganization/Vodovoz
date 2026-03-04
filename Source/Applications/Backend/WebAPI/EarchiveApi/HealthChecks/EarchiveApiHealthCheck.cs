using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Providers;

namespace EarchiveApi.HealthChecks
{
	public class EarchiveApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly IConfiguration _configuration;

		public EarchiveApiHealthCheck(ILogger<EarchiveApiHealthCheck> logger,  IConfiguration configuration, IUnitOfWorkFactory unitOfWorkFactory,
			IHealthCheckServiceInfoProvider serviceInfoProvider)
			: base(logger, serviceInfoProvider, unitOfWorkFactory)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			var healthSection = _configuration.GetSection("Health");
			var serviceAddress = healthSection.GetValue<string>("BaseAddress");

			var channel = GrpcChannel.ForAddress(serviceAddress);

			var earchiveUpdClient = new EarchiveApiTestClient.EarchiveUpd.EarchiveUpdClient(channel);

			var response = earchiveUpdClient.GetAddresses(new EarchiveApiTestClient.CounterpartyInfo { Id = 2 });

			await response.ResponseStream.MoveNext(CancellationToken.None);

			var address = response.ResponseStream.Current;

			return new VodovozHealthResultDto { IsHealthy = address != null };
		}
	}
}
