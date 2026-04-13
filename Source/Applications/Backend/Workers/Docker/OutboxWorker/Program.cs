using CustomerPushNotifications.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Net.Security;
using System.Security.Authentication;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Persistence;

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
				.ConfigureServices((hostContext, services) =>
				{
					services.Configure<CustomerNotificationTransportSettings>(hostContext.Configuration.GetSection("CustomerNotificationTransportSettings"));

					services.AddMassTransit(busConf =>
					{
						busConf.ConfigureCustomerNotificationRabbitMq(services, hostContext.Configuration);

					});
					
					services.AddScoped<IOutboxRepository, OutboxRepository>();

					services.AddHostedService<OutboxWorker>();

				});
	}
}
