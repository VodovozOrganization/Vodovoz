using VodovozHealthCheck;
using VodovozHealthCheck.Utils;

namespace PayPageAPI.HealthChecks
{
	public class PayPageHealthCheck : VodovozHealthCheckBase
	{
		protected override VodovozHealthResultDto GetHealthResult()
		{
			var isHealthy = UrlExistsChecker.UrlExists("https://sbp.vodovoz-spb.ru:4001/f9758536-733e-479d-9190-888d76572400");

			return new VodovozHealthResultDto
			{
				IsHealthy = isHealthy
			};
		}
	}
}
