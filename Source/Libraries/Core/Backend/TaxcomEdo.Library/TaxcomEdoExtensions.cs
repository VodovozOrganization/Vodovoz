using System.Security.Authentication;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using TaxcomEdo.Library.Options;
using Vodovoz.Core.Data.Documents;
using Vodovoz.Settings.Pacs;

namespace TaxcomEdo.Library
{
	public static class TaxcomEdoExtensions
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
							hostConfigurator.UseSsl(ssl => ssl.Protocol = SslProtocols.Tls12);
						}
					});

				configurator.Send<InfoForCreatingDocumentEdo>(x => x.UseRoutingKeyFormatter(y => nameof(y)));
				configurator.Message<InfoForCreatingDocumentEdo>(x => x.SetEntityName("taxcomedo-docflow"));
				configurator.Publish<InfoForCreatingDocumentEdo>(x =>
				{
					x.ExchangeType = ExchangeType.Direct;
					x.Durable = true;
					x.AutoDelete = false;
					x.BindQueue(
						"taxcomedo-docflow",
						"order-info-for-upd",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = nameof(InfoForCreatingEdoUpd);
						});
					x.BindQueue(
						"taxcomedo-docflow",
						"order-info-for-bills",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = nameof(InfoForCreatingEdoBill);
						});
					x.BindQueue(
						"taxcomedo-docflow",
						"order-info-for-bills-without-shipment",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = nameof(InfoForCreatingBillWithoutShipmentEdo);
						});
				});

				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}
		
		public static IServiceCollection AddPreparerDependencyGroup(this IServiceCollection services)
		{
			return services;
		}
		
		public static IServiceCollection ConfigurePreparer(this IServiceCollection services, IConfiguration configuration) =>
			services
				.Configure<TaxcomEdoOptions>(o => configuration.GetSection(nameof(o)))
				.Configure<DocumentFlowOptions>(o => configuration.GetSection(nameof(o)));
	}
}
