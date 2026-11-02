using System;
using System.Net.Security;
using System.Security.Authentication;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Library.Options;
using Vodovoz.Settings.Pacs;

namespace TaxcomEdo.Library
{
	public static class TaxcomEdoLibraryExtensions
	{
		public static IServiceCollection ConfigurePreparer(this IServiceCollection services, IConfiguration configuration) =>
			services
				.Configure<TaxcomEdoOptions>(configuration.GetSection(TaxcomEdoOptions.Path))
				.Configure<DocumentFlowOptions>(configuration.GetSection(DocumentFlowOptions.Path));

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
				
				configurator.ConfigureTopologyForEdoUpd();

				configurator.Publish<InfoForCreatingDocumentEdo>(x => x.Exclude = true);
				configurator.Publish<InfoForCreatingDocumentEdoWithAttachment>(x => x.Exclude = true);

				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}

		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForEdoUpd(this IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<InfoForCreatingEdoUpd>(x => x.SetEntityName(InfoForCreatingEdoUpd.ExchangeAndQueueName));

			configurator.Publish<InfoForCreatingEdoUpd>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					InfoForCreatingEdoUpd.ExchangeAndQueueName,
					InfoForCreatingEdoUpd.ExchangeAndQueueName);
			});
			
			return configurator;
		}
	}
}
