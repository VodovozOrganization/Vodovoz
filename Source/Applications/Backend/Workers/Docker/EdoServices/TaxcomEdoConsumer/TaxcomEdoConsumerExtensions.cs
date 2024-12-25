using System;
using System.Net.Security;
using System.Security.Authentication;
using Edo.Contracts.Messages.Events;
using Edo.Docflow.Taxcom;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
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
				.AddScoped<IEdoDocflowHandler, EdoDocflowHandler>()
				.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWorkFactory>().CreateWithoutRoot());
			
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
			this IRabbitMqBusFactoryConfigurator configurator, string edoAccount)
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
		
		private static IRabbitMqBusFactoryConfigurator ConfigureTopologyForTaxcomDocflowSendEvent(
			this IRabbitMqBusFactoryConfigurator configurator, string edoAccount)
		{
			configurator.Send<TaxcomDocflowSendEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));
			configurator.Message<TaxcomDocflowSendEvent>(x => x.SetEntityName(TaxcomDocflowSendEvent.Event));
			configurator.Publish<TaxcomDocflowSendEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
			});
			
			return configurator;
		}
	}
}
