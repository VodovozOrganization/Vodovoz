using Core.Infrastructure;
using MassTransit;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;

namespace Pacs.Admin.Client.Consumers.Definitions
{
	public class SettingsConsumerDefinition : ConsumerDefinition<SettingsConsumer>
	{
		private readonly int _administratorId;

		public SettingsConsumerDefinition(IPacsAdministratorProvider adminProvider)
		{
			if(adminProvider.AdministratorId == null)
			{
				throw new PacsInitException("Невозможно подключить получение сообщений администратора, " +
					"так как текущий пользователь не является администратором СКУД.");
			}

			_administratorId = adminProvider.AdministratorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.settings.consumer-admin-{_administratorId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<SettingsConsumer> consumerConfigurator)
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
