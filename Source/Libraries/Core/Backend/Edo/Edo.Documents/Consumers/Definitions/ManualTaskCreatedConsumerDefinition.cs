using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	/// <summary>
	/// Конфигурация потребителя события создания задачи на ручную отправку документов по ЭДО
	/// </summary>
	public class ManualTaskCreatedConsumerDefinition : ConsumerDefinition<ManualTaskCreatedConsumer>
	{
		public ManualTaskCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.manual-task-created.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<ManualTaskCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<ManualTaskCreatedEvent>();
			}
		}
	}
}
