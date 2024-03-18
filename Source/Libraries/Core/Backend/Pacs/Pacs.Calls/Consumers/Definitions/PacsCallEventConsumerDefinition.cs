using Core.Infrastructure;
using MassTransit;
using Pacs.Core;
using RabbitMQ.Client;
using CallEvent = Pacs.Core.Messages.Events.CallEvent;

namespace Pacs.Calls.Consumers.Definitions
{
	public class PacsCallEventConsumerDefinition : ConsumerDefinition<PacsCallEventConsumer>
	{
		private readonly int _administratorId;

		public PacsCallEventConsumerDefinition(IPacsAdministratorProvider administratorProvider)
		{
			if(administratorProvider == null)
			{
				throw new PacsInitException("Невозможно получение событий звонков. Так как в системе не определен администратор.");
			}
			_administratorId = administratorProvider.AdministratorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.call.consumer-admin-{_administratorId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<PacsCallEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<CallEvent>();
			}
		}
	}
}
