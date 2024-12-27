using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Net.Security;
using System.Security.Authentication;
using Edo.Contracts.Messages.Events;
using Vodovoz.Settings.Pacs;

namespace Edo.Transport
{
	public static class TransportConfiguration
	{
		public static void AddEdoTopology(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
		{
			// rename exchange: edo.customer-request-created.publish
			cfg.Message<EdoRequestCreatedEvent>(x => x.SetEntityName("edo.event.request_created"));
			cfg.Publish<EdoRequestCreatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			// rename exchange: edo.docflow-updated.publish
			cfg.Message<EdoDocflowUpdatedEvent>(x => x.SetEntityName("EdoDocflowUpdated"));
			cfg.Publish<EdoDocflowUpdatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<DocumentTaskCreatedEvent>(x => x.SetEntityName("edo.document-task-created.publish"));
			cfg.Publish<DocumentTaskCreatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<TransferRequestCreatedEvent>(x => x.SetEntityName("edo.transfer-request-created.publish"));
			cfg.Publish<TransferRequestCreatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<TransferTaskReadyToSendEvent>(x => x.SetEntityName("edo.transfer-task-ready-to-send.publish"));
			cfg.Publish<TransferTaskReadyToSendEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<TransferDocumentAcceptedEvent>(x => x.SetEntityName("edo.transfer-document-accepted.publish"));
			cfg.Publish<TransferDocumentAcceptedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<TransferDocumentSendEvent>(x => x.SetEntityName("edo.transfer-document-send.publish"));
			cfg.Publish<TransferDocumentSendEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<TransferDoneEvent>(x => x.SetEntityName("edo.transfer-done.publish"));
			cfg.Publish<TransferDoneEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<TransferDoneEvent>(x => x.SetEntityName("edo.transfer-done.publish"));
			cfg.Publish<TransferDoneEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<CustomerDocumentSendEvent>(x => x.SetEntityName("edo.customer-document-send.publish"));
			cfg.Publish<CustomerDocumentSendEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<CustomerDocumentSentEvent>(x => x.SetEntityName("edo.customer-document-sent.publish"));
			cfg.Publish<CustomerDocumentSentEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<CustomerDocumentAcceptedEvent>(x => x.SetEntityName("edo.customer-document-accepted.publish"));
			cfg.Publish<CustomerDocumentAcceptedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
			
			AddTaxcomEdoTopology(cfg);
		}

		public static IServiceCollection AddEdoMassTransit(
			this IServiceCollection services,
			Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureRabbit = null,
			Action<IBusRegistrationConfigurator> configureBus = null)
		{
			services.AddMassTransit(busCfg =>
			{
				configureBus?.Invoke(busCfg);

				busCfg.UsingRabbitMq((context, rabbitCfg) =>
				{
					var messageSettings = context.GetRequiredService<IMessageTransportSettings>();
					rabbitCfg.Host(messageSettings.Host, (ushort)messageSettings.Port, messageSettings.VirtualHost,
						rabbitHostCfg =>
						{
							rabbitHostCfg.Username(messageSettings.Username);
							rabbitHostCfg.Password(messageSettings.Password);
							if(messageSettings.UseSSL)
							{
								rabbitHostCfg.UseSsl(ssl =>
								{
									if(Enum.TryParse<SslPolicyErrors>(messageSettings.AllowSslPolicyErrors, out var allowedPolicyErrors))
									{
										ssl.AllowPolicyErrors(allowedPolicyErrors);
									}

									ssl.Protocol = SslProtocols.Tls12;
								});
							}
						}
					);

					//rabbitCfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));

					rabbitCfg.AddEdoTopology(context);

					configureRabbit?.Invoke(context, rabbitCfg);

					rabbitCfg.ConfigureEndpoints(context);
				});
			});

			return services;
		}

		public static void AddTaxcomEdoTopology(this IRabbitMqBusFactoryConfigurator cfg)
		{
			cfg.Send<TaxcomDocflowSendEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));
			cfg.Message<TaxcomDocflowSendEvent>(x => x.SetEntityName($"{TaxcomDocflowSendEvent.Event}.publish"));
			cfg.Publish<TaxcomDocflowSendEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Send<OutgoingTaxcomDocflowUpdatedEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));
			cfg.Message<OutgoingTaxcomDocflowUpdatedEvent>(x => x.SetEntityName($"{OutgoingTaxcomDocflowUpdatedEvent.Event}.publish"));
			cfg.Publish<OutgoingTaxcomDocflowUpdatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Send<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));
			cfg.Message<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>(x =>
				x.SetEntityName($"{AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent.Event}.publish"));
			cfg.Publish<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}
}
