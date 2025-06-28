using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Withdrawal.Consumers.Definitions
{
	public class WithdrawalTaskCreatedConsumerDefinition : ConsumerDefinition<WithdrawalTaskCreatedConsumer>
	{
		public WithdrawalTaskCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.withdrawal_task_created_event.consumer.withdrawal");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<WithdrawalTaskCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<WithdrawalTaskCreatedEvent>();
			}
		}
	}
}
