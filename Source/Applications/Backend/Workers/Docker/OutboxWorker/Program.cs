using CustomerNotifications.Contracts;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;

namespace OutboxWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((ctx, builder) =>
				{
					builder.AddNLog(ctx.Configuration.GetSection("NLog"));
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddOutboxWorker(
						hostContext.Configuration,
						contractAssemblies: new[]
						{
							typeof(CustomerNotificationIntegrationEvent).Assembly,
							typeof(EdoNotifications.Contracts.AssemblyFinder).Assembly,
						},
						transportSectionName: "NotificationTransportSettings");
				});
	}
}
