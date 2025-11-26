using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.InformalOrderDocuments.Consumers.Definitions
{
	/// <summary>
	/// Настройка MassTransit для события создания задачи по неформализованному документу заказа
	/// </summary>
	public class InformalOrderDocumenTaskCreatedConsumerDefinition : ConsumerDefinition<InformalOrderDocumenTaskCreatedConsumer>
	{
		public InformalOrderDocumenTaskCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.informal-order-document-task-created.consumer.informal-order-documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<InformalOrderDocumenTaskCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<InformalOrderDocumenTaskCreatedEvent>();
			}
		}
	}
}
