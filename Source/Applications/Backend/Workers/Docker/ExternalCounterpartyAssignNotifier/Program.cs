using System;
using ExternalCounterpartyAssignNotifier.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ExternalCounterpartyAssignNotifier
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
					services.AddLogging(loggingBuilder =>
					{
						loggingBuilder.ClearProviders();
						loggingBuilder.AddNLog("NLog.config");
					});
					
					services.AddHostedService<ExternalCounterpartyAssignNotifier>();
					
					services.AddHttpClient<INotificationService, NotificationService>(c =>
					{
						//c.BaseAddress = new Uri(hostContext.Configuration.GetSection("VodovozSiteNotificationService").GetValue<string>("BaseUrl"));
						c.DefaultRequestHeaders.Add("Accept", "application/json");
					});
				});
	}
}
