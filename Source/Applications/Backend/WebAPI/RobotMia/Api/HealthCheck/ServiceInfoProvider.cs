using VodovozHealthCheck.Providers;

namespace Vodovoz.RobotMia.Api.HealthCheck
{
	public class ServiceInfoProvider : IHealthCheckServiceInfoProvider
	{
		public string DetailedDescription => "";

		public string Name => "Апи робота Мия";
	}
}
