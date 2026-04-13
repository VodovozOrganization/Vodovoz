using CustomerPushNotifications.Transport;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Net.Security;
using System.Security.Authentication;

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
				.ConfigureServices((hostContext, services) =>
				{
					services.Configure<CustomerNotificationTransportSettings>(
						hostContext.Configuration.GetSection("CustomerNotificationTransportSettings"));

					services.AddMassTransit(x =>
					{
						x.AddConsumer<CustomerPushNotificationsConsumer, CustomerPushNotificationsConsumerDefinition>();

						x.ConfigureCustomerNotificationRabbitMq(services, hostContext.Configuration);

					});
				});
	}
}
