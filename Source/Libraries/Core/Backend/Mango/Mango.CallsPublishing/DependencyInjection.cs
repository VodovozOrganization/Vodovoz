using Mango.Core.Handlers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Authentication;
using Vodovoz.Settings.Pacs;

namespace Mango.CallsPublishing
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCallsPublishing(this IServiceCollection services)
		{
			services.AddScoped<ICallEventHandler, PublisherCallEventHandler>();

			services.AddMassTransit(busCfg =>
			{
				busCfg.UsingRabbitMq((context, rabbitCfg) =>
				{
					var ts = context.GetRequiredService<IMessageTransportSettings>();
					rabbitCfg.Host(ts.Host, (ushort)ts.Port, ts.VirtualHost,
						rabbitHostCfg =>
						{
							rabbitHostCfg.Username(ts.Username);
							rabbitHostCfg.Password(ts.Password);
							if(ts.UseSSL)
							{
								rabbitHostCfg.UseSsl(ssl => ssl.Protocol = SslProtocols.Tls12);
							}
						}
					);

					rabbitCfg.AddMangoProducerTopology(context);
					rabbitCfg.ConfigureEndpoints(context);
				});
			});

			return services;
		}
	}
}
