using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.HealthChecks
{
	public class TaxcomEdoApiHealthCheck : IHealthCheck
	{
		private readonly ILogger<TaxcomEdoApiHealthCheck> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public TaxcomEdoApiHealthCheck(
			ILogger<TaxcomEdoApiHealthCheck> logger,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}
		
		public async Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = new CancellationToken())
		{
			_logger.LogInformation("Поступил запрос на информацию о работоспособности.");
			VodovozHealthResultDto healthResult;
			
			try
			{
				healthResult = await GetHealthResult();
			}
			catch(Exception e)
			{
				return HealthCheckResult.Unhealthy("Возникло исключение во время проверки работоспособности.", e);
			}

			if(healthResult == null)
			{
				return HealthCheckResult.Unhealthy("Пустой результат проверки.");
			}

			if(healthResult.IsHealthy)
			{
				return HealthCheckResult.Healthy("Проверка пройдена успешно");
			}

			var unhealthyDictionary = new Dictionary<string, object>
			{
				{ "results", healthResult.AdditionalUnhealthyResults }
			};

			const string failedMessage = "Проверка не пройдена";

			_logger.LogInformation(failedMessage);

			return HealthCheckResult.Unhealthy(failedMessage, null, unhealthyDictionary);
		}

		private async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			
			var healthResult = new VodovozHealthResultDto();

			const string getStatusEndpoint = "GetStatus";

			var response = await HttpResponseHelper.GetByUri(
				$"{baseAddress}/api/{getStatusEndpoint}",
				_httpClientFactory);

			var isHealthy = ((HttpResponseMessage)response).StatusCode == HttpStatusCode.OK;

			if(!isHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add($"{getStatusEndpoint} не прошёл проверку.");
			}

			healthResult.IsHealthy = isHealthy;

			return healthResult;
		}
	}
}
