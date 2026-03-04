using VodovozHealthCheck.Providers;

namespace BitrixApi.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "Api Bitrix";
	}
}
