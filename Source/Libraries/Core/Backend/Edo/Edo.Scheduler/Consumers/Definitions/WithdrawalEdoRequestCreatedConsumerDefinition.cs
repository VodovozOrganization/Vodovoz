using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Scheduler.Consumers.Definitions
{
	/// <summary>
	/// Определение consumer для обработки события создания заявки на вывод из оборота
	/// </summary>
	public class WithdrawalEdoRequestCreatedConsumerDefinition : ConsumerDefinition<WithdrawalEdoRequestCreatedConsumer>
	{
		public WithdrawalEdoRequestCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.withdrawal-request-created.consumer.scheduler");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<WithdrawalEdoRequestCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<WithdrawalEdoRequestCreatedEvent>();
			}
		}
	}
}
