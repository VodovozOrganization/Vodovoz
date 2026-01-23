using Google.Protobuf.WellKnownTypes;
using VodovozHealthCheck.Providers;

namespace TaxcomEdoApi.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "Api Такском";
	}
}
