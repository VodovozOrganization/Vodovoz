using VodovozHealthCheck.Providers;

namespace UnsubscribePage.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "Страница отписки от email рассылок";
	}
}
