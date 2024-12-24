using System;
using System.Net.Security;
using System.Security.Authentication;
using Edo.Transport.Messages.Events;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Vodovoz.Settings.Pacs;

namespace TaxcomEdo.Library
{
	public static class TaxcomEdoDocflowUpdaterExtensions
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
					.ConfigureTopologyForAcceptingIngoingTaxcomDocflowWaitingForSignatureEvent()
					.ConfigureTopologyForOutgoingTaxcomDocflowUpdatedEvent();

				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}

		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForAcceptingIngoingTaxcomDocflowWaitingForSignatureEvent(
			this IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Send<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));
			configurator.Message<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>(x => x.SetEntityName(AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent.Event));
			configurator.Publish<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
			});
			
			return configurator;
		}
		
		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForOutgoingTaxcomDocflowUpdatedEvent(
			this IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Send<OutgoingTaxcomDocflowUpdatedEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));
			configurator.Message<OutgoingTaxcomDocflowUpdatedEvent>(x => x.SetEntityName(OutgoingTaxcomDocflowUpdatedEvent.Event));
			configurator.Publish<OutgoingTaxcomDocflowUpdatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
			});
			
			return configurator;
		}
	}
}
