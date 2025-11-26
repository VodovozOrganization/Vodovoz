using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.InformalOrderDocuments.Consumers.Definitions
{
	/// <summary>
	/// Конфигурация потребителя события принятия документа
	/// </summary>
	public class OrderDocumentAcceptedConsumerDefinition : ConsumerDefinition<InformalOrderDocumentAcceptedConsumer>
	{
		public OrderDocumentAcceptedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.informal-document-accepted.consumer.informal-order-documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<InformalOrderDocumentAcceptedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<InformalOrderDocumentAcceptedEvent>();
			}
		}
	}
}
