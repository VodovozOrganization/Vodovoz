using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VodovozHealthCheck
{
	public abstract class VodovozHealthCheckBase : IHealthCheck
	{
		private readonly ILogger<VodovozHealthCheckBase> _logger =  new Logger<VodovozHealthCheckBase>(new NLogLoggerFactory());

		//public VodovozHealthCheckBase(ILogger<VodovozHealthCheckBase> logger )
		//{
		//	_logger = logger?? throw new ArgumentNullException(nameof(logger));
		//}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
		{
			_logger.LogInformation("Поступил запрос на информацию о здоровье.");

			VodovozHealthResultDto healthResult = null;
			try
			{
				healthResult = await GetHealthResult();
			}
			catch { }


			if(healthResult == null)
			{
				var exceptionMessage = "Возникло искючение во время проверки здоровья.";

				_logger.LogInformation(exceptionMessage);

				return HealthCheckResult.Unhealthy(exceptionMessage);
			}

			if(healthResult.IsHealthy )
			{
				return HealthCheckResult.Healthy();
			}

			var unhealthyDictionary = new Dictionary<string, object>
			{
				{ "results", healthResult.AdditionalResults }
			};

			var failedMessage = "Проверка не пройдена";

			_logger.LogInformation(failedMessage);

			return HealthCheckResult.Unhealthy(failedMessage, null, unhealthyDictionary);
		}

		protected abstract Task<VodovozHealthResultDto> GetHealthResult();

	}
}
