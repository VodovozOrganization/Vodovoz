using Autofac.Extensions.DependencyInjection;
using DeliveryRulesService.Workers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System.Threading.Tasks;

namespace DeliveryRulesService
{
	public class Program
    {
        public static async Task Main(string[] args)
        {
			await CreateHostBuilder(args).Build().RunAsync();
		}

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
				{
                    webBuilder.UseStartup<Startup>();
                })
				.ConfigureServices(services =>
				{
					services.AddHostedService<DistrictCacheWorker>();
				})
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
					logging.SetMinimumLevel(LogLevel.Trace);
				})
				.UseNLog();
    }
}
