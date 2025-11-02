using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	public class OrderDocumentProblemConsumerDefinition : ConsumerDefinition<OrderDocumentProblemConsumer>
	{
		public OrderDocumentProblemConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.customer-document-problem.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OrderDocumentProblemConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OrderDocumentProblemEvent>();
			}
		}
	}
}
