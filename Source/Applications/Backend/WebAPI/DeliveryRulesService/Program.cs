using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using System;
using System.Threading.Tasks;

namespace DeliveryRulesService
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			await CreateHostBuilder(args).Build().RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((hostBuilderContext, logging) =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(hostBuilderContext.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
