using VodovozHealthCheck;

namespace TaxcomEdoApi.HealthChecks
{
	public class TaxcomEdoApiHealthCheck : VodovozHealthCheckBase
	{
		protected override VodovozHealthResultDto GetHealthResult()
		{
			var healthResult = new VodovozHealthResultDto
			{
				IsHealthy = IsHealthy
			};

			return healthResult;
		}

		public bool IsHealthy { get; set; }
	}
}
