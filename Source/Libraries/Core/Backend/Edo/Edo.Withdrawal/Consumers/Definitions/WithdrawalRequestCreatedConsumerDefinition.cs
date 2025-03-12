using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Withdrawal.Consumers.Definitions
{
	public class WithdrawalRequestCreatedConsumerDefinition : ConsumerDefinition<WithdrawalRequestCreatedConsumer>
	{
		public WithdrawalRequestCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.withdrawal_task_created_event.consumer.withdrawal");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<WithdrawalRequestCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<EdoRequestCreatedEvent>();
			}
		}
	}
}
