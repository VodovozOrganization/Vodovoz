using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	public class TransferDoneConsumerDefinition : ConsumerDefinition<TransferDoneConsumer>
	{
		public TransferDoneConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-done.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferDoneConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferDoneEvent>();
			}
		}
	}
}
