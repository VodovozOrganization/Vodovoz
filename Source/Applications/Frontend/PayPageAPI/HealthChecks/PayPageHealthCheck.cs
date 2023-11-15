using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace PayPageAPI.HealthChecks
{
	public class PayPageHealthCheck : VodovozHealthCheckBase
	{
		private readonly IConfiguration _configuration;

		public PayPageHealthCheck(IConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override  Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var isHealthy = ResponseHelper.CheckUriExists($"{baseAddress}/f9758536-733e-479d-9190-888d76572400");

			return Task.FromResult(new VodovozHealthResultDto
			{
				IsHealthy = isHealthy
			});
		}
	}
}
