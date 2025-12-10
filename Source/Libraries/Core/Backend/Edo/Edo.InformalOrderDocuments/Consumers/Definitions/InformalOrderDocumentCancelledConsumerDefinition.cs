using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.InformalOrderDocuments.Consumers.Definitions
{
	/// <summary>
	/// Конфигурация консьюмера события аннулирования документа
	/// </summary>
	public class InformalOrderDocumentCancelledConsumerDefinition : ConsumerDefinition<InformalOrderDocumentCancelledConsumer>
	{
		public InformalOrderDocumentCancelledConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.informal-document-cancelled.consumer.informal-order-documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<InformalOrderDocumentCancelledConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Durable = true;
				rmq.Bind<InformalOrderDocumentCancelledEvent>();
			}
		}
	}
}
