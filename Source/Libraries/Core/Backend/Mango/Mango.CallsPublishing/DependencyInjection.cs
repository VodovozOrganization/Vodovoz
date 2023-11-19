using Mango.Core.Handlers;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Authentication;

namespace Mango.CallsPublishing
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddCallsPublishing(this IServiceCollection services, IMessageTransportSettings transportSettings)
		{
			services
				.AddScoped<ICallEventHandler, PublisherCallEventHandler>()
				;

			services.AddMassTransit(x =>
			{
				x.UsingRabbitMq((context, cfg) =>
				{
					cfg.Host(
						transportSettings.Host,
						(ushort)transportSettings.Port,
						transportSettings.VirtualHost,
						hostCfg => 
						{
							hostCfg.Username(transportSettings.Username);
							hostCfg.Password(transportSettings.Password);
							hostCfg.UseSsl(ssl =>
							{
								ssl.Protocol = SslProtocols.Tls12;
							});
						}
					);

					cfg.ConfigureMangoProducerTopology(context);

					cfg.ConfigureEndpoints(context);
				});
			});

			return services;
		}
	}
}
