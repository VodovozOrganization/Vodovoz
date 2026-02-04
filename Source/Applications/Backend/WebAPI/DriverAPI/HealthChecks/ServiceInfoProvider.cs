using VodovozHealthCheck.Providers;

namespace DriverAPI.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "Api для приложения водителей";
	}
}
