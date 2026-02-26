using VodovozHealthCheck.Providers;

namespace FastPaymentsAPI.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "Сервис СБП Авангарда для оплаты по QR";

		public string Name => "Сервис СБП Авангарда для оплаты по QR";
	}
}
