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
				
				configurator
					.ConfigureTopologyForEdoUpd()
					.ConfigureTopologyForEdoBill()
					.ConfigureTopologyForBillWithoutShipmentForDebtEdo()
					.ConfigureTopologyForBillWithoutShipmentForPaymentEdo()
					.ConfigureTopologyForBillWithoutShipmentForAdvancePaymentEdo();

				configurator.Publish<InfoForCreatingDocumentEdo>(x => x.Exclude = true);
				configurator.Publish<InfoForCreatingDocumentEdoWithAttachment>(x => x.Exclude = true);
				configurator.Publish<InfoForCreatingBillWithoutShipmentEdo>(x => x.Exclude = true);

				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}

		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForEdoBill(this IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<InfoForCreatingEdoBill>(x => x.SetEntityName(InfoForCreatingEdoBill.ExchangeAndQueueName));

			configurator.Publish<InfoForCreatingEdoBill>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					InfoForCreatingEdoBill.ExchangeAndQueueName,
					InfoForCreatingEdoBill.ExchangeAndQueueName);
			});
			
			return configurator;
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

		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForBillWithoutShipmentForDebtEdo(
			this IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<InfoForCreatingBillWithoutShipmentForDebtEdo>(x =>
				x.SetEntityName(InfoForCreatingBillWithoutShipmentForDebtEdo.ExchangeAndQueueName));

			configurator.Publish<InfoForCreatingBillWithoutShipmentForDebtEdo>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					InfoForCreatingBillWithoutShipmentForDebtEdo.ExchangeAndQueueName,
					InfoForCreatingBillWithoutShipmentForDebtEdo.ExchangeAndQueueName);
			});

			return configurator;
		}
		
		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForBillWithoutShipmentForPaymentEdo(
			this IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<InfoForCreatingBillWithoutShipmentForPaymentEdo>(x =>
				x.SetEntityName(InfoForCreatingBillWithoutShipmentForPaymentEdo.ExchangeAndQueueName));

			configurator.Publish<InfoForCreatingBillWithoutShipmentForPaymentEdo>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					InfoForCreatingBillWithoutShipmentForPaymentEdo.ExchangeAndQueueName,
					InfoForCreatingBillWithoutShipmentForPaymentEdo.ExchangeAndQueueName);
			});

			return configurator;
		}
		
		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForBillWithoutShipmentForAdvancePaymentEdo(
			this IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Message<InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo>(x =>
				x.SetEntityName(InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo.ExchangeAndQueueName));

			configurator.Publish<InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo.ExchangeAndQueueName,
					InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo.ExchangeAndQueueName);
			});

			return configurator;
		}
	}
}
