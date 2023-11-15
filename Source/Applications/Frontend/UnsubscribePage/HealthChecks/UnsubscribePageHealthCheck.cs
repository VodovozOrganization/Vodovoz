using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Utils;

namespace UnsubscribePage.HealthChecks
{
	public class UnsubscribePageHealthCheck : VodovozHealthCheckBase
	{
		private readonly IConfiguration _configuration;

		public UnsubscribePageHealthCheck(IConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");

			var isHealthy = UrlExistsChecker.UrlExists($"{baseAddress}/1049b7ef-825b-46b7-87c9-b234af7f6d5e");

			return Task.FromResult(new VodovozHealthResultDto
			{
				IsHealthy = isHealthy
			});
		}
	}
}
