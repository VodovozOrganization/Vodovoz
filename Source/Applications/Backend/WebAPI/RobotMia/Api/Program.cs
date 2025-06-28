using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Vodovoz.Presentation.WebApi;

namespace Vodovoz.RobotMia.Api
{
	/// <summary>
	/// Api интеграции с роботом Мия
	/// </summary>
	public class Program
	{
		private const string _nLogSectionName = nameof(NLog);

		/// <summary>
		/// Точка входа
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args)
		{
			try
			{
				CreateHostBuilder(args).Build().Run();
			}
			finally
			{
				// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
				LogManager.Shutdown();
			}
		}

		/// <summary>
		/// Создание Api хоста
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(configuration =>
				{
					configuration.ConfigureJsonSourcesAutoReload();
					configuration.AddJsonFile("/run/secrets/secrets.json", optional: true, reloadOnChange: true);
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureLogging((context, logging) =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(context.Configuration.GetSection(_nLogSectionName));
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
