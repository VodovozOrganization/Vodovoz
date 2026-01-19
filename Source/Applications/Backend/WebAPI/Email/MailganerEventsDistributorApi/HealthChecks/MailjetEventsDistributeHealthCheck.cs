using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace MailganerEventsDistributorApi.HealthChecks
{
	public class MailjetEventsDistributeHealthCheck : VodovozHealthCheckBase
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;

		public MailjetEventsDistributeHealthCheck(
			ILogger<MailjetEventsDistributeHealthCheck> logger,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHttpClientFactory httpClientFactory)
		: base(logger, unitOfWorkFactory)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");

			var isHealthy = await HttpResponseHelper.CheckUriExistsAsync($"{baseAddress}/Test", _httpClientFactory);

			return new VodovozHealthResultDto
			{
				IsHealthy = isHealthy
			};
		}
	}
}
