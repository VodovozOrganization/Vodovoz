using VodovozHealthCheck.Providers;

namespace DeliveryRulesService.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription =>
			"Получение правил доставки по координатам. " +
			"Получение правил доставки и тарифной зоны по координатам. " +
			"Получение правил доставки по координатам и номенклатурам. " +
			"Получение правил доставки с ДЗЧ по координатам и номенклатурам.";

		public string Name => "Правила доставки";
	}
}
