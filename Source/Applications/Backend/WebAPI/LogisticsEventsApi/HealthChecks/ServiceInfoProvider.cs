using VodovozHealthCheck.Providers;

namespace LogisticsEventsApi.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "Api для логистических событий";
	}
}
