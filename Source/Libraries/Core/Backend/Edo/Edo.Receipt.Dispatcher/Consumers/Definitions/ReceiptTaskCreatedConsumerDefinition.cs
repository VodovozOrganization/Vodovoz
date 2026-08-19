using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Receipt.Dispatcher.Consumers.Definitions
{
	public class ReceiptTaskCreatedConsumerDefinition : ConsumerDefinition<ReceiptTaskCreatedConsumer>
	{
		public ReceiptTaskCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.receipt-task-created.consumer.receipt-dispatcher");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ReceiptTaskCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			// Возможное исключение гонки при назначении document_index
			// фискальных документов (см. UNIQUE KEY index_unique на edo_fiscal_documents)
			endpointConfigurator.PrefetchCount = 1;
			endpointConfigurator.ConcurrentMessageLimit = 1;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Bind<ReceiptTaskCreatedEvent>();
				//rmq.DiscardFaultedMessages();
			}
		}
	}
}
