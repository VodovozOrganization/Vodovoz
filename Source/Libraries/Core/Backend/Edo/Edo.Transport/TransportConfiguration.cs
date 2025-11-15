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
			cfg.Message<EdoRequestCreatedEvent>(x => x.SetEntityName("edo.customer-request-created.publish"));
			cfg.Publish<EdoRequestCreatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<EdoDocflowUpdatedEvent>(x => x.SetEntityName("edo.docflow-updated.publish"));
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
			
			cfg.Message<TenderTaskCreatedEvent>(x => x.SetEntityName("edo.tender-task-created.publish"));
			cfg.Publish<TenderTaskCreatedEvent>(x =>
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

			cfg.Message<TransferTaskPrepareToSendEvent>(x => x.SetEntityName("edo.transfer-task-prepare-to-send.publish"));
			cfg.Publish<TransferTaskPrepareToSendEvent>(x =>
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

			cfg.Message<TransferDocumentCancelledEvent>(x => x.SetEntityName("edo.transfer-document-cancelled.publish"));
			cfg.Publish<TransferDocumentCancelledEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<TransferDocumentProblemEvent>(x => x.SetEntityName("edo.transfer-document-problem.publish"));
			cfg.Publish<TransferDocumentProblemEvent>(x =>
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

			cfg.Message<TransferCompleteEvent>(x => x.SetEntityName("edo.transfer-complete.publish"));
			cfg.Send<TransferCompleteEvent>(x => x.UseRoutingKeyFormatter(c => c.Message.TransferInitiator.ToString()));
			cfg.Publish<TransferCompleteEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<OrderDocumentSendEvent>(x => x.SetEntityName("edo.order-document-send.publish"));
			cfg.Publish<OrderDocumentSendEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<OrderDocumentSentEvent>(x => x.SetEntityName("edo.order-document-sent.publish"));
			cfg.Publish<OrderDocumentSentEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<OrderDocumentAcceptedEvent>(x => x.SetEntityName("edo.order-document-accepted.publish"));
			cfg.Publish<OrderDocumentAcceptedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<OrderDocumentCancelledEvent>(x => x.SetEntityName("edo.order-document-cancelled.publish"));
			cfg.Publish<OrderDocumentCancelledEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<OrderDocumentProblemEvent>(x => x.SetEntityName("edo.order-document-problem.publish"));
			cfg.Publish<OrderDocumentProblemEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<ReceiptReadyToSendEvent>(x => x.SetEntityName("edo.receipt-ready-to-send.publish"));
			cfg.Publish<ReceiptReadyToSendEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<ReceiptCompleteEvent>(x => x.SetEntityName("edo.receipt-complete.publish"));
			cfg.Publish<ReceiptCompleteEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<SaveCodesTaskCreatedEvent>(x => x.SetEntityName("edo.codes-save-task-created.publish"));
			cfg.Publish<SaveCodesTaskCreatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<ReceiptTaskCreatedEvent>(x => x.SetEntityName("edo.receipt-task-created.publish"));
			cfg.Publish<ReceiptTaskCreatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<EquipmentTransferTaskCreatedEvent>(x => x.SetEntityName("edo.equipment-transfer-task-created.publish"));
			cfg.Publish<EquipmentTransferTaskCreatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<WithdrawalTaskCreatedEvent>(x => x.SetEntityName("edo.withdrawal_task_created_event.publish"));
			cfg.Publish<WithdrawalTaskCreatedEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});
			
			cfg.Message<RequestTaskCancellationEvent>(x => x.SetEntityName("edo.request-task-cancellation.publish"));
			cfg.Publish<RequestTaskCancellationEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Fanout;
				x.Durable = true;
				x.AutoDelete = false;
			});

			cfg.Message<RequestDocflowCancellationEvent>(x => x.SetEntityName("edo.request-docflow-cancellation.publish"));
			cfg.Publish<RequestDocflowCancellationEvent>(x =>
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

					rabbitCfg.UseMessageRetry(r => 
					{
						r.Interval(5, TimeSpan.FromSeconds(5));

						// Если не ставить ничего, то все будут обрабатываться
						// Если установить Handle, то остальные будут игнорироваться
						// Если установить Ignore, то будут обрабатываться все кроме игнорируемых
						// Если установить Handle и Ignore, то будут обрабатываться только те которые в Handle

						// По умолчанию желательно игнорировать только те,
						// по которым точно будет одинаковый результат
						r.Ignore<NotImplementedException>();
						r.Ignore<NotSupportedException>();
						r.Ignore<ArgumentNullException>();
					});

					rabbitCfg.AddEdoTopology(context);

					configureRabbit?.Invoke(context, rabbitCfg);

					rabbitCfg.ConfigureEndpoints(context);
				});
			});

			services.AddScoped<MessageService>();

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

			cfg.Send<AcceptingWaitingForCancellationDocflowEvent>(x => x.UseRoutingKeyFormatter(y => y.Message.EdoAccount));
			cfg.Message<AcceptingWaitingForCancellationDocflowEvent>(x =>
				x.SetEntityName($"{AcceptingWaitingForCancellationDocflowEvent.Event}.publish"));
			cfg.Publish<AcceptingWaitingForCancellationDocflowEvent>(x =>
			{
				x.ExchangeType = ExchangeType.Direct;
				x.Durable = true;
				x.AutoDelete = false;
			});
		}
	}
}
