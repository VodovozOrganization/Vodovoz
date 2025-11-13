using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Transfer.Dispatcher.Consumers.Definitions
{
	public class TransferDocumentProblemConsumerDefinition : ConsumerDefinition<TransferDocumentProblemConsumer>
	{
		public TransferDocumentProblemConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-document-problem.consumer.transfer-dispatcher");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferDocumentProblemConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferDocumentProblemEvent>();
			}
		}
	}
}
