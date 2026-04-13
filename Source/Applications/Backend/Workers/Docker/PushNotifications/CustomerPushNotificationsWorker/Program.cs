using CustomerPushNotifications.Transport;
using MassTransit;
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

						x.UsingRabbitMq((context, cfg) =>
						{
							var settings = context.GetRequiredService<IOptions<CustomerNotificationTransportSettings>>().Value;

							cfg.Host(settings.Host, (ushort)settings.Port, settings.VirtualHost, h =>
							{
								h.Username(settings.Username);
								h.Password(settings.Password);

								if(settings.UseSSL)
								{
									h.UseSsl(ssl =>
									{
										if(Enum.TryParse<SslPolicyErrors>(settings.AllowSslPolicyErrors, out var allowed))
											ssl.AllowPolicyErrors(allowed);

										ssl.Protocol = SslProtocols.Tls12;
									});
								}
							});
							cfg.ConfigureEndpoints(context);
						});
					});
				});
	}
}
