using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vodovoz.Settings.Metrics;

namespace Vodovoz.Zabbix.Sender
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureZabbixSender(this IServiceCollection services, string workerName)
		{
			services
				.AddSingleton<IZabbixSender, VodovozZabbixSender>(x=>
				{
					var logger = x.GetRequiredService<ILogger<VodovozZabbixSender>>();
					var metricSettings = x.GetRequiredService<IMetricSettings>();

					return new VodovozZabbixSender(workerName, metricSettings, logger);
				});
			
			return services;
		}
	}
}
