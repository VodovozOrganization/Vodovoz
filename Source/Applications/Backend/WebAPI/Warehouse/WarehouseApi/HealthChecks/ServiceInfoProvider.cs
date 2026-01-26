using VodovozHealthCheck.Providers;

namespace WarehouseApi.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "API для приложения склада";
	}
}
