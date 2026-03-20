using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	/// <summary>
	/// Определение consumer для обработки события завершения документооборота
	/// </summary>
	public class OrderDocflowCompletedConsumerDefinition : ConsumerDefinition<OrderDocflowCompletedConsumer>
	{
		public OrderDocflowCompletedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.order-docflow-completed.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OrderDocflowCompletedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OrderDocflowCompletedEvent>();
			}
		}
	}
}
