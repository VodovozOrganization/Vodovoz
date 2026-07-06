using CustomerNotifications.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Persistence;
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
					services.Configure<CustomerNotificationTransportSettings>(hostContext.Configuration.GetSection("CustomerNotificationTransportSettings"));

					services.AddMassTransit(busConf =>
					{
						busConf.ConfigureCustomerNotificationsRabbitMq(services, hostContext.Configuration);

					});

					services.AddScoped<IOutboxRepository, OutboxRepository>();

					services.AddHostedService<OutboxWorker>();

				});
	}
}
