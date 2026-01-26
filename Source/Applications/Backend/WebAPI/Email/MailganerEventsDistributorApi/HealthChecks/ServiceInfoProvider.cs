using VodovozHealthCheck.Providers;

namespace MailganerEventsDistributorApi.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "Api email сервиса";
	}
}
