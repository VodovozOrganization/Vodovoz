using EdoAutoSendReceiveWorker.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TaxcomEdo.Client;
using Vodovoz.Settings.Metrics;
using Vodovoz.Zabbix.Sender;

namespace EdoAutoSendReceiveWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddHttpClient()
						.AddTaxcomClient()
						.Configure<TaxcomEdoAutoSendReceiveWorkerOptions>(
							hostContext.Configuration.GetSection(TaxcomEdoAutoSendReceiveWorkerOptions.Path))
						.Configure<MetricOptions>(hostContext.Configuration.GetSection(MetricOptions.Path))
						.AddSingleton<IMetricSettings>(sp =>
						{
							var settings = sp.GetRequiredService<IOptions<MetricOptions>>().Value;
							return settings;
						})
						.ConfigureZabbixSender(nameof(TaxcomEdoAutoSendReceiveWorker))
						.AddHostedService<TaxcomEdoAutoSendReceiveWorker>();
				});
	}
}
