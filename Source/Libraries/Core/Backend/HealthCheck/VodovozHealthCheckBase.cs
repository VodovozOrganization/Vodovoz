using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting.Internal;
using VodovozHealthCheck.Dto;

namespace VodovozHealthCheck
{
	public abstract class VodovozHealthCheckBase : IHealthCheck
	{
		private readonly ILogger<VodovozHealthCheckBase> _logger =  new Logger<VodovozHealthCheckBase>(new NLogLoggerFactory());
		private readonly IHttpClientFactory _httpClientFactory;

		//public static HttpClient HttpClient { get; set; }

		//public VodovozHealthCheckBase(ILogger<VodovozHealthCheckBase> logger )
		//{
		//	_logger = logger?? throw new ArgumentNullException(nameof(logger));
		//}


		public VodovozHealthCheckBase(IHttpClientFactory httpClientFactory = null)
		{
			_httpClientFactory = httpClientFactory;
			
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
		{
			_logger.LogInformation("Поступил запрос на информацию о здоровье.");

			VodovozHealthResultDto healthResult = null;
			try
			{
				healthResult = await GetHealthResult();
			}
			catch(Exception e)
			{
				return HealthCheckResult.Unhealthy("Возникло искючение во время проверки здоровья.", e);
			}

			if(healthResult == null )
			{
				return HealthCheckResult.Unhealthy("Пустой результат проверки.");
			}

			if(healthResult.IsHealthy )
			{
				return HealthCheckResult.Healthy();
			}

			var unhealthyDictionary = new Dictionary<string, object>
			{
				{ "results", healthResult.AdditionalUnhealthyResults }
			};

			var failedMessage = "Проверка не пройдена";

			_logger.LogInformation(failedMessage);

			return HealthCheckResult.Unhealthy(failedMessage, null, unhealthyDictionary);
		}

		protected abstract Task<VodovozHealthResultDto> GetHealthResult();

	}
}
