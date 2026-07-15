using VodovozHealthCheck.Providers;

namespace CustomerAppsApi.V2.HealthChecks
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "Сервис регистрации/авторизации клиентов";

		public string Name => "Сервис регистрации/авторизации клиентов";
	}
}
