using EdoAutoSendReceiveWorker.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaxcomEdo.Client;

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
						.AddHostedService<TaxcomEdoAutoSendReceiveWorker>();
				});
	}
}
