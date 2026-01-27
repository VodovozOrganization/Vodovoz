using VodovozHealthCheck.Providers;

namespace RoboatsService.HealthCheck
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "Api робоатс";
	}
}
