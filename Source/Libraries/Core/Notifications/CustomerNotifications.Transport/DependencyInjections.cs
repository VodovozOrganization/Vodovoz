using System;
using System.Net.Security;
using System.Security.Authentication;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CustomerNotifications.Transport
{
	public static class DependencyInjections
	{
		public static IBusRegistrationConfigurator ConfigureCustomerNotificationsRabbitMq(
			this IBusRegistrationConfigurator busConf,
			IServiceCollection serviceCollection,
			IConfiguration configuration)
		{
			serviceCollection.Configure<CustomerNotificationTransportSettings>(
				configuration.GetSection(nameof(CustomerNotificationTransportSettings)));

			busConf.UsingRabbitMq((context, configurator) =>
			{
				var settings = context.GetRequiredService<IOptions<CustomerNotificationTransportSettings>>().Value;

				configurator.Host(settings.Host, (ushort)settings.Port, settings.VirtualHost, hostConfigurator =>
				{
					hostConfigurator.Username(settings.Username);
					hostConfigurator.Password(settings.Password);

					if(settings.UseSSL)
					{
						hostConfigurator.UseSsl(ssl =>
						{
							if(Enum.TryParse<SslPolicyErrors>(settings.AllowSslPolicyErrors, out var allowedPolicyErrors))
							{
								ssl.AllowPolicyErrors(allowedPolicyErrors);
							}

							ssl.Protocol = SslProtocols.Tls12;
						});
					}
				});

				configurator.ConfigureEndpoints(context); //Art8m в воркере?
			});

			return busConf;
		}
	}
}

