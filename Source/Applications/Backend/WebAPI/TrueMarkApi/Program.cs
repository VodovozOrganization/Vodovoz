using Autofac.Extensions.DependencyInjection;
using DatabaseServiceWorker;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace TrueMarkApi
{
	public class Program
	{
		private const string _nlogSectionName = nameof(NLog);

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((context, logging) =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(context.Configuration.GetSection(_nlogSectionName));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				})
			.ConfigureServices((hostContext, services) =>
			{
				services.ConfigureTrueMarkWorker(hostContext);
			});
	}
}
