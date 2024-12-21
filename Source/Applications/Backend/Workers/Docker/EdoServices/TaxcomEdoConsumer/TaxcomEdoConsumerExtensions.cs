using System;
using System.Net.Security;
using System.Security.Authentication;
using Edo.Docflow.Taxcom;
using Edo.Transport2;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TaxcomEdoConsumer.Options;
using Vodovoz.Settings.Pacs;

namespace TaxcomEdoConsumer
{
	public static class TaxcomEdoConsumerExtensions
	{
		public static IServiceCollection AddTaxcomEdoConsumerDependenciesGroup(this IServiceCollection services)
		{
			services
				.AddScoped<IEdoDocflowHandler, EdoDocflowHandler>();
			
			return services;
		}
		
		public static IBusRegistrationConfigurator ConfigureRabbitMq(this IBusRegistrationConfigurator busConf)
		{
			busConf.UsingRabbitMq((context, configurator) =>
			{
				var messageSettings = context.GetRequiredService<IMessageTransportSettings>();
				var edoAccount = context.GetRequiredService<IOptions<TaxcomEdoConsumerOptions>>()
					.Value
					.EdoAccount;

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
					.ConfigureTopologyForAcceptingIngoingTaxcomDocflowWaitingForSignatureEvent(edoAccount)
					.ConfigureTopologyForOutgoingTaxcomDocflowUpdatedEvent(edoAccount)
					.ConfigureTopologyForEdoDocflowUpdatedEvent()
					.ConfigureTopologyForTaxcomDocflowSendEvent(edoAccount)
					;

				configurator.ConfigureEndpoints(context);
			});
			
			return busConf;
		}

		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForAcceptingIngoingTaxcomDocflowWaitingForSignatureEvent(
			this IRabbitMqBusFactoryConfigurator configurator, string edoAccount)
		{
			configurator.Send<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));
			configurator.ReceiveEndpoint($"{edoAccount}_{AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent.Event}", e =>
			{
				e.Bind(AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent.Event, x =>
				{
					x.Durable = true;
					x.AutoDelete = false;
					x.ExchangeType = ExchangeType.Direct;
					x.RoutingKey = $"{edoAccount}";
				});
			});
			
			return configurator;
		}
		
		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForOutgoingTaxcomDocflowUpdatedEvent(
			this IRabbitMqBusFactoryConfigurator configurator, string edoAccount)
		{
			configurator.Send<OutgoingTaxcomDocflowUpdatedEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));
			configurator.ReceiveEndpoint($"{edoAccount}_{OutgoingTaxcomDocflowUpdatedEvent.Event}", e =>
			{
				e.Bind(OutgoingTaxcomDocflowUpdatedEvent.Event, x =>
				{
					x.Durable = true;
					x.AutoDelete = false;
					x.ExchangeType = ExchangeType.Direct;
					x.RoutingKey = $"{edoAccount}";
				});
			});
			
			return configurator;
		}
		
		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForEdoDocflowUpdatedEvent(
			this IRabbitMqBusFactoryConfigurator configurator)
		{
			configurator.Publish<EdoDocflowUpdatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue($"{EdoDocflowUpdatedEvent.Event}", $"{EdoDocflowUpdatedEvent.Event}");
			});
			
			return configurator;
		}
		
		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForTaxcomDocflowSendEvent(
			this IRabbitMqBusFactoryConfigurator configurator, string edoAccount)
		{
			configurator.Send<TaxcomDocflowSendEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));
			configurator.ReceiveEndpoint($"{edoAccount}_{TaxcomDocflowSendEvent.Event}", e =>
			{
				e.Bind(TaxcomDocflowSendEvent.Event, x =>
				{
					x.Durable = true;
					x.AutoDelete = false;
					x.ExchangeType = ExchangeType.Direct;
					x.RoutingKey = $"{edoAccount}";
				});
			});
			
			return configurator;
		}
	}
}
