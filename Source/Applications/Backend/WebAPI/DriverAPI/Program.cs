using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace DriverAPI
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			try
			{
				CreateHostBuilder(args).Build().Run();
				Console.WriteLine("Application started successfully.");
			}
			finally
			{
				// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
				NLog.LogManager.Shutdown();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				})
				.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));
	}
}
