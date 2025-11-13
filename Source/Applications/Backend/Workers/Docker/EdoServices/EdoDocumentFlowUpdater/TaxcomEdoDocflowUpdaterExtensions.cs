using System;
using System.Net.Security;
using System.Security.Authentication;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Settings.Pacs;

namespace TaxcomEdo.Library
{
	public static class TaxcomEdoDocflowUpdaterExtensions
	{
		public static IBusRegistrationConfigurator ConfigureRabbitMq(this IBusRegistrationConfigurator busConf)
		{
			busConf.UsingRabbitMq((context, configurator) =>
			{
				var messageSettings = context.GetRequiredService<IMessageTransportSettings>();

				configurator.Host(
					messageSettings.Host,
					(ushort)messageSettings.Port,
					messageSettings.VirtualHost, hostConfigurator =>
					{
						hostConfigurator.Username(messageSettings.Username);
						hostConfigurator.Password(messageSettings.Password);

						if(messageSettings.UseSSL)
						{
							hostConfigurator.UseSsl(ssl =>
							{
								if(Enum.TryParse<SslPolicyErrors>(messageSettings.AllowSslPolicyErrors, out var allowedPolicyErrors))
								{
									ssl.AllowPolicyErrors(allowedPolicyErrors);
								}

								ssl.Protocol = SslProtocols.Tls12;
							});
						}
					});
				
				configurator.AddTaxcomEdoTopology();
				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}
	}
}
