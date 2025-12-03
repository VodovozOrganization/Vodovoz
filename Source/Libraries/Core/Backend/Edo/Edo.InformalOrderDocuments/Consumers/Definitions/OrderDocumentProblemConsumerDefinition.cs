using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.InformalOrderDocuments.Consumers.Definitions
{
	/// <summary>
	/// Конфигурация потребителя события проблемы с документом заказа
	/// </summary>
	public class OrderDocumentProblemConsumerDefinition : ConsumerDefinition<InformalOrderDocumentProblemConsumer>
	{
		public OrderDocumentProblemConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.informal-document-problem.consumer.informal-order-documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<InformalOrderDocumentProblemConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Durable = true;
				rmq.Bind<InformalOrderDocumentProblemEvent>();
			}
		}
	}
}
