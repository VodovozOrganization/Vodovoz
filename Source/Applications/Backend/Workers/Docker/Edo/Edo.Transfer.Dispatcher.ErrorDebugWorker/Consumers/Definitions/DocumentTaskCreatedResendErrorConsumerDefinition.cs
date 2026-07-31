using MassTransit;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers.Definitions
{
	public class DocumentTaskCreatedResendErrorConsumerDefinition : ConsumerDefinition<DocumentTaskCreatedResendErrorConsumer>
	{
		public DocumentTaskCreatedResendErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.document-task-created.consumer.documents_error_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<DocumentTaskCreatedResendErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 10;
		}
	}

}
