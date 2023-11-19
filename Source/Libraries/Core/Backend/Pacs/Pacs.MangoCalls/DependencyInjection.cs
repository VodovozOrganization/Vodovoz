using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Pacs.MangoCalls.Services;
using System.Reflection;
using System.Security.Authentication;
using Vodovoz.Settings.Pacs;

namespace Pacs.MangoCalls
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsMangoCallsServices(this IServiceCollection services, IMessageTransportSettings transportSettings)
		{
			services
				.AddScoped<ICallEventSequenceValidator, CallEventSequenceValidator>()
				.AddScoped<ICallEventRegistrar, CallEventRegistrar>()

				.AddMassTransit(x =>
				{
					x.AddConsumers(Assembly.GetExecutingAssembly());
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
								if(transportSettings.UseSSL)
								{
									hostCfg.UseSsl(ssl =>
									{
										ssl.Protocol = SslProtocols.Tls12;
									});
								}
							}
						);

						cfg.ConfigureCallsTopology(context);
						cfg.ConfigureEndpoints(context);
					});
				})
			;

			return services;
		}
	}
}
