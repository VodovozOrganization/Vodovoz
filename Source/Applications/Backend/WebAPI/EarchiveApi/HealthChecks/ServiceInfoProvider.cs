using VodovozHealthCheck.Providers;

namespace EarchiveApi.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "Api для электронного архива";
	}
}
