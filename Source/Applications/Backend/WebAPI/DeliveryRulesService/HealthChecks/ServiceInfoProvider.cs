using VodovozHealthCheck.Providers;

namespace DeliveryRulesService.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "Сервис правил доставки";

		public string Name => "Правила доставки";
	}
}
