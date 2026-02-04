using VodovozHealthCheck.Providers;

namespace PayPageAPI.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "Открыте платёжной страницы";

		public string Name => "Платежная страница для оплат";
	}
}
