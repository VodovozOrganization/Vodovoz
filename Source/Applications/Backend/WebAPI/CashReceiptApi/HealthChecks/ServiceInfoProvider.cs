using VodovozHealthCheck.Providers;

namespace CashReceiptApi.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "API для работы с кассовыми чеками";
	}
}
