using VodovozHealthCheck.Providers;

namespace MailjetEventsDistributorAPI.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => throw new System.NotImplementedException();
	}
}
