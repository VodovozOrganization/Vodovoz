using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace PayPageAPI.HealthChecks
{
	public class PayPageHealthCheck : VodovozHealthCheckBase
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;

		public PayPageHealthCheck(
			ILogger<PayPageHealthCheck> logger,
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
			var guid = healthSection.GetValue<string>("Variables:Guid");

			var isHealthy = await HttpResponseHelper.CheckUriExistsAsync($"{baseAddress}/{guid}", _httpClientFactory);

			var result = new VodovozHealthResultDto
			{
				IsHealthy = isHealthy,				 
			};

			if(!isHealthy)
			{
				result.AdditionalUnhealthyResults.Add("Платёжная страница недоступна");
			}

			return result;
		}
	}
}
