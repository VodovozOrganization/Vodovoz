using System.Security.Authentication;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Library.Options;
using Vodovoz.Settings.Pacs;

namespace TaxcomEdo.Library
{
	public static class TaxcomEdoExtensions
	{
		private const string _taxcomEdoDocFlowExchange = "taxcom-edo-docflow";
		private const string _taxcomEdoContactsExchange = "taxcom-edo-contacts";
		
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
				configurator.Message<InfoForCreatingDocumentEdo>(x => x.SetEntityName(_taxcomEdoDocFlowExchange));
				configurator.Publish<InfoForCreatingDocumentEdo>(x =>
				{
					x.ExchangeType = ExchangeType.Direct;
					x.Durable = true;
					x.AutoDelete = false;
					x.BindQueue(
						_taxcomEdoDocFlowExchange,
						"info-for-create-upd",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = nameof(InfoForCreatingEdoUpd);
						});
					x.BindQueue(
						_taxcomEdoDocFlowExchange,
						"info-for-create-bills",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = nameof(InfoForCreatingEdoBill);
						});
					x.BindQueue(
						_taxcomEdoDocFlowExchange,
						"info-for-create-bills-without-shipment",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Direct;
							conf.RoutingKey = nameof(InfoForCreatingBillWithoutShipmentEdo);
						});
				});
				
				configurator.Message<EdoContactInfo>(x => x.SetEntityName(_taxcomEdoContactsExchange));
				configurator.Publish<EdoContactInfo>(x =>
				{
					x.ExchangeType = ExchangeType.Fanout;
					x.Durable = true;
					x.AutoDelete = false;
					x.BindQueue(
						_taxcomEdoContactsExchange,
						"contacts-info",
						conf =>
						{
							conf.ExchangeType = ExchangeType.Fanout;
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
