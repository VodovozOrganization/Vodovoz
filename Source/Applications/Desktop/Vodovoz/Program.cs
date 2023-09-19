using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Vodovoz
{
	public class Program
	{
		[STAThread]
		public static void Main (string[] args)
		{
			Gtk.Application.Init();

			var host = CreateHostBuilder(args).Build();

			host.Services.GetService<Startup>().Start(args);
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			new HostBuilder()
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddJsonFile("appsettings.json");
				})
				.ConfigureServices((hostingContext, services) =>
				{
					services.AddLogging(options =>
					{
						options.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					});

					services.AddSingleton<Startup>();
				})
				.ConfigureLogging((hostContext, logging) =>
				{
					logging.AddConsole();
				});
	}
}
