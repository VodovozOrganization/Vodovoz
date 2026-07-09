using CustomerNotifications.Transport;
using CustomerNotificationsWorker.Config;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;

namespace CustomerNotificationsWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
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
					services.Configure<CustomerNotificationTransportSettings>(
						hostContext.Configuration.GetSection("CustomerNotificationTransportSettings"));

					services.AddMassTransit(x =>
					{
						x.AddConsumer<CustomerNotificationsConsumer, CustomerNotificationsConsumerDefinition>();

						x.ConfigureCustomerNotificationsRabbitMq(services, hostContext.Configuration);
					});

					services.Configure<NotifierOptions>(hostContext.Configuration.GetSection(NotifierOptions.Path));

					services.AddHttpClient();
				});
	}
}
