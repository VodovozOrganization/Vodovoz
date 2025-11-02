using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Settings.Metrics;

namespace Vodovoz.Zabbix.Sender
{
	public static class DependencyInjection
	{
		public static IServiceCollection ConfigureZabbixSenderFromDataBase(this IServiceCollection services, string workerName)
		{
			services
				.AddSingleton<IMetricSettings, MetricSettings>()
				.AddSingleton<IZabbixSender, VodovozZabbixSender>(x =>
				{
					var logger = x.GetRequiredService<ILogger<VodovozZabbixSender>>();
					var metricSettings = x.GetRequiredService<IMetricSettings>();
					var hostEnvironment = x.GetRequiredService<IHostEnvironment>();

					return new VodovozZabbixSender(workerName, metricSettings, logger, hostEnvironment);
				});

			return services;
		}

		public static IServiceCollection ConfigureZabbixSenderFromAppSettings(this IServiceCollection services, string workerName, HostBuilderContext hostContext)
		{
			services
				.AddSingleton<IZabbixSender, VodovozZabbixSender>(x =>
				{
					var logger = x.GetRequiredService<ILogger<VodovozZabbixSender>>();

					var hostEnvironment = x.GetRequiredService<IHostEnvironment>();

					var metricSettingsSection = hostContext.Configuration.GetSection(nameof(MetricSettings));
					var metricSettings = new MetricSettings
					{
						ZabbixHost = metricSettingsSection["ZabbixHost"],
						ZabbixUrl = metricSettingsSection["ZabbixUrl"],
						ZabbixNeedSendMetrics = Convert.ToBoolean(metricSettingsSection["ZabbixNeedSendMetrics"])
					};

					return new VodovozZabbixSender(workerName, metricSettings, logger, hostEnvironment);
				});

			return services;
		}
	}
}
