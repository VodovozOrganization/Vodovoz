using CustomerNotifications.Contracts;
using Edo.Contracts;
using EdoNotifications.Contracts;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using Vodovoz.Zabbix.Sender;
using Edo.Transport;

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
						outBoxTransportBuilder => outBoxTransportBuilder
							.Add<INotificationBus>
							(
								"NotificationTransportSettings",
								new[]
								{
									typeof(CustomerNotificationIntegrationEvent).Assembly,
									typeof(EdoNotificationAssemblyFinder).Assembly
								}
							)
							.Add<IPacsBus>
							(
								"PacsTransportSettings",
								new[]
								{
									typeof(EdoContractsAssemblyFinder).Assembly
								},
								configureTopology: (context, cfg) => cfg.AddEdoTopology(context)
							)
						);

					services.ConfigureZabbixSenderFromAppSettings(hostContext);
				});
	}
}
