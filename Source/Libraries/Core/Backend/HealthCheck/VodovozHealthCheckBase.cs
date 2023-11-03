using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck.Utils;

namespace VodovozHealthCheck
{
	public abstract class VodovozHealthCheckBase : IHealthCheck
	{
		public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
		{
			VodovozHealthResultDto healthResult = null;
			try
			{
				healthResult = GetHealthResult();
			}
			catch { }


			if(healthResult == null)
			{
				return Task.FromResult(HealthCheckResult.Unhealthy("Вознмикло искючение во время проверки."));
			}

			if(healthResult.IsHealthy )
			{
				return Task.FromResult(HealthCheckResult.Healthy());
			}

			var unhealthyDictionary = new Dictionary<string, object>
			{
				{ "results", healthResult.AdditionalResults }
			};

			return Task.FromResult(HealthCheckResult.Unhealthy("Проверка не пройдена.", null, unhealthyDictionary));
		}

		protected abstract VodovozHealthResultDto GetHealthResult();

	}
}
