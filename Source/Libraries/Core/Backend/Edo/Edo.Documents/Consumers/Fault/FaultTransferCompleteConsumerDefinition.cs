using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents.Consumers.Fault
{
	public class FaultTransferCompleteConsumerDefinition : ConsumerDefinition<FaultTransferCompleteConsumer>
	{
		public FaultTransferCompleteConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-complete.consumer.documents_fault");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<FaultTransferCompleteConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferCompleteEvent>(x =>
				{
					x.RoutingKey = TransferInitiator.Document.ToString();
				});
			}
		}
	}
}
