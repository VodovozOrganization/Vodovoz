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
			var exchange = "AcceptingIngoingTaxcomDocflowWaitingForSignature";
			configurator.Send<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));

			configurator.Publish<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					exchange,
					$"{edoAccount}_{nameof(AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent)}",
					conf =>
					{
						conf.ExchangeType = ExchangeType.Direct;
						conf.RoutingKey = $"{edoAccount}";
					});
			});
			
			return configurator;
		}
		
		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForOutgoingTaxcomDocflowUpdatedEvent(
			this IRabbitMqBusFactoryConfigurator configurator, string edoAccount)
		{
			var exchange = "OutgoingTaxcomDocflowUpdated";
			configurator.Send<OutgoingTaxcomDocflowUpdatedEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));

			configurator.Publish<OutgoingTaxcomDocflowUpdatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					exchange,
					$"{edoAccount}_{nameof(OutgoingTaxcomDocflowUpdatedEvent)}",
					conf =>
					{
						conf.ExchangeType = ExchangeType.Direct;
						conf.RoutingKey = $"{edoAccount}";
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
				x.BindQueue(nameof(EdoDocflowUpdatedEvent), nameof(EdoDocflowUpdatedEvent));
			});
			
			return configurator;
		}
		
		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForTaxcomDocflowSendEvent(
			this IRabbitMqBusFactoryConfigurator configurator, string edoAccount)
		{
			configurator.Send<TaxcomDocflowSendEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));

			configurator.Publish<TaxcomDocflowSendEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
				x.BindQueue(
					nameof(TaxcomDocflowSendEvent),
					$"{edoAccount}_{nameof(TaxcomDocflowSendEvent)}",
					conf =>
					{
						conf.ExchangeType = ExchangeType.Direct;
						conf.RoutingKey = $"{edoAccount}";
					});
			});
			
			return configurator;
		}
	}
}
