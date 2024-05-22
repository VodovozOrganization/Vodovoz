using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Settings.Metrics;
using Vodovoz.Zabbix.Sender;

namespace DatabaseServiceWorker
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureZabbixSender(this IServiceCollection services, string workerName)
		{
			services
				.AddSingleton<IZabbixSender, VodovozZabbixSender>(x =>
					new VodovozZabbixSender(workerName, x.GetRequiredService<IMetricSettings>()));

			return services;
		}
	}
}
