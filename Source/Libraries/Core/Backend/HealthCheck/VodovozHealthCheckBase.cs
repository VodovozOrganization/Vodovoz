using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck.Dto;

namespace VodovozHealthCheck
{
	public abstract class VodovozHealthCheckBase : IHealthCheck
	{
		private readonly ILogger<VodovozHealthCheckBase> _logger;

		public VodovozHealthCheckBase(ILogger<VodovozHealthCheckBase> logger)
		{
			_logger = logger;
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new ())
		{
			_logger.LogInformation("Поступил запрос на информацию о здоровье.");

			VodovozHealthResultDto healthResult;

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
