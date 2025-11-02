using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace LogisticsEventsApi
{
	public class Program
	{
		private const string _nLogSectionName = nameof(NLog);

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((hostBuilderContext, logging) =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(hostBuilderContext.Configuration
						.GetSection(_nLogSectionName));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
	}
}
