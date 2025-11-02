using Core.Infrastructure;
using MassTransit;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;

namespace Pacs.Admin.Client.Consumers.Definitions
{
	public class OperatorStateAdminConsumerDefinition : ConsumerDefinition<OperatorStateAdminConsumer>
	{
		private readonly int _adminId;

		public OperatorStateAdminConsumerDefinition(IPacsAdministratorProvider adminProvider)
		{
			if(adminProvider.AdministratorId == null)
			{
				throw new PacsInitException("Невозможно подключить получение уведомлений от всех операторов, " +
					"так как текущий пользователь не является администратором СКУД.");
			}
			_adminId = adminProvider.AdministratorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.operator_state.consumer-admin-{_adminId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OperatorStateAdminConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Exclusive = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OperatorStateEvent>(c =>
				{
					c.RoutingKey = $"pacs.operator.state.#";
				});
			}
		}
	}
}
