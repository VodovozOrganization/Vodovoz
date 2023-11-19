using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Authentication;
using Vodovoz.Settings.Pacs;

namespace Pacs.Admin.Server
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsAdminServices(this IServiceCollection services, IMessageTransportSettings transportSettings)
		{
			services.AddControllers();

			services
				.AddScoped<ISettingsNotifier, SettingsNotifier>()
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
							if(transportSettings.UseSSL)
							{
								hostCfg.UseSsl(ssl =>
								{
									ssl.Protocol = SslProtocols.Tls12;
								});
							}
						}
					);

					cfg.ConfigureAdminMessagesPublishTopology(context);

					cfg.ConfigureEndpoints(context);
				});
			});

			return services;
		}
	}
}
