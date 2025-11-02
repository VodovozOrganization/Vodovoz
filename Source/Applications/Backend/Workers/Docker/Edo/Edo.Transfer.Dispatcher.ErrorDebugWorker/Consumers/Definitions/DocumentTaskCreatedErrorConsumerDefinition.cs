using Edo.Contracts.Messages.Events;
using MassTransit;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions
{
	public class DocumentTaskCreatedErrorConsumerDefinition : ConsumerDefinition<DocumentTaskCreatedErrorConsumer>
	{
		public DocumentTaskCreatedErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.document-task-created.consumer.documents_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<DocumentTaskCreatedErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}

	
}
