using Core.Infrastructure;
using MassTransit;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;

namespace Pacs.Operators.Client.Consumers.Definitions
{
	public class OperatorSettingsConsumerDefinition : ConsumerDefinition<OperatorSettingsConsumer>
	{
		private readonly int _operatorId;

		public OperatorSettingsConsumerDefinition(IPacsOperatorProvider operatorProvider)
		{
			if(operatorProvider.OperatorId == null)
			{
				throw new PacsInitException("Невозможно подключить получение настроек СКУД, " +
					"так как текущий пользователь не является оператором СКУД.");
			}

			_operatorId = operatorProvider.OperatorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.settings.consumer-operator-{_operatorId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OperatorSettingsConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Exclusive = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<SettingsEvent>();
			}
		}
	}
}
