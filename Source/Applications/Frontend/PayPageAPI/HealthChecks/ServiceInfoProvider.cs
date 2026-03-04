using VodovozHealthCheck.Providers;

namespace PayPageAPI.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "Открытие платёжной страницы";

		public string Name => "Платежная страница для оплат";
	}
}
