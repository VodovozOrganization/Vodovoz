using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Authentication;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Settings.Pacs;

namespace Pacs.Operator.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsOperatorClient(this IServiceCollection services, IMessageTransportSettings transportSettings)
		{
			services
				.AddScoped<IOperatorClient, OperatorClient>()
				.AddScoped<OperatorStateConsumer>()
				.AddScoped<IConsumer<OperatorState>, OperatorStateConsumer>()
				.AddScoped<IObservable<OperatorState>, OperatorStateConsumer>()
				.AddScoped<IOperatorClientFactory, OperatorClientFactory>()
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

					cfg.ConfigureOperatorMessageTopology(context);

					cfg.ConfigureEndpoints(context);
				});
			});

			return services;
		}
	}
}
