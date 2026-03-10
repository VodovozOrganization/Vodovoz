using VodovozHealthCheck.Providers;

namespace CustomerOrdersApi.HealthCheck
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "Сервис регистрации онлайн заказов, заявок на звонок";

		public string Name => "Сервис регистрации онлайн заказов, заявок на звонок";
	}
}
