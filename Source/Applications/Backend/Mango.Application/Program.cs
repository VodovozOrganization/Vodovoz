using Mango.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mango.Application
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
					logging.AddConsole();
					logging.AddDebug();
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<MangoStatsWorker>();
					services.AddMangoServices(hostContext.Configuration);
				});
	}
}
