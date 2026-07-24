using CustomerNotificationsWorker.Config;
using MassTransit;
using MessageTransport.MassTransit;
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
					services.AddMassTransit(x =>
					{
						x.AddConsumer<CustomerNotificationsConsumer, CustomerNotificationsConsumerDefinition>();

						x.ConfigureRabbitMq(services, hostContext.Configuration, "NotificationTransportSettings");
					});

					services.Configure<NotifierOptions>(hostContext.Configuration.GetSection(NotifierOptions.Path));

					services.AddHttpClient();
				});
	}
}
