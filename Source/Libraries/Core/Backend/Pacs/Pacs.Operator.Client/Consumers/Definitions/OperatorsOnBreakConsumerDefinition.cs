using Core.Infrastructure;
using MassTransit;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;

namespace Pacs.Operators.Client.Consumers.Definitions
{
	public class OperatorsOnBreakConsumerDefinition : ConsumerDefinition<OperatorsOnBreakConsumer>
	{
		private readonly int _operatorId;

		public OperatorsOnBreakConsumerDefinition(IPacsOperatorProvider operatorProvider)
		{
			if(operatorProvider.OperatorId == null)
			{
				throw new PacsInitException("Невозможно получение состояния оператора. Так как в системе не определен оператор.");
			}
			_operatorId = operatorProvider.OperatorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.operators_on_break.consumer-operator-{_operatorId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OperatorsOnBreakConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Exclusive = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OperatorsOnBreakEvent>();
			}
		}
	}
}
