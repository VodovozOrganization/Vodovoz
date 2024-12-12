using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Security;
using System;
using System.Security.Authentication;
using Vodovoz.Settings.Pacs;

namespace Mango.CallsPublishing
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCallsPublishing(this IServiceCollection services)
		{
			services.AddMassTransit(busCfg =>
			{
				busCfg.UsingRabbitMq((context, rabbitCfg) =>
				{
					var messageSettings = context.GetRequiredService<IMessageTransportSettings>();
					rabbitCfg.Host(messageSettings.Host, (ushort)messageSettings.Port, messageSettings.VirtualHost,
						rabbitHostCfg =>
						{
							rabbitHostCfg.Username(messageSettings.Username);
							rabbitHostCfg.Password(messageSettings.Password);
							if(messageSettings.UseSSL)
							{
								rabbitHostCfg.UseSsl(ssl =>
								{
									if(Enum.TryParse<SslPolicyErrors>(messageSettings.AllowSslPolicyErrors, out var allowedPolicyErrors))
									{
										ssl.AllowPolicyErrors(allowedPolicyErrors);
									}

									ssl.Protocol = SslProtocols.Tls12;
								});
							}
						}
					);

					rabbitCfg.AddMangoTopology(context);
					rabbitCfg.ConfigureEndpoints(context);
				});
			});

			return services;
		}
	}
}
