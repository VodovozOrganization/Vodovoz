using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Utils;

namespace MailjetEventsDistributorAPI.HealthChecks
{
	public class MailjetEventsDistributeHealthCheck : VodovozHealthCheckBase
	{
		private readonly IConfiguration _configuration;

		public MailjetEventsDistributeHealthCheck(IConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");

			var isHealthy = UrlExistsChecker.UrlExists($"{baseAddress}/Test");

			return new VodovozHealthResultDto
			{
				IsHealthy = isHealthy
			};
		}
	}
}
