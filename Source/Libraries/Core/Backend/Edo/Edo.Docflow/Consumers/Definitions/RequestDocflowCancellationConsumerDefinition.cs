using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Definitions
{
	public class RequestDocflowCancellationConsumerDefinition : ConsumerDefinition<RequestDocflowCancellationConsumer>
	{
		public RequestDocflowCancellationConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.request-docflow-cancellation.consumer.docflow");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<RequestDocflowCancellationConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<RequestDocflowCancellationEvent>();
			}
		}
	}
}
