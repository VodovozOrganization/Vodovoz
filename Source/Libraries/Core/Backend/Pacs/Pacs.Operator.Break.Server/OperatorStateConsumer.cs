using Core.Infrastructure;
using MassTransit;
using Pacs.Operators.Server;
using RabbitMQ.Client;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Operators.Break.Server
{
	public class OperatorStateConsumer : IConsumer<OperatorState>
	{
		private readonly GlobalBreakController _breaksController;

		public OperatorStateConsumer(GlobalBreakController breaksController)
		{
			_breaksController = breaksController ?? throw new System.ArgumentNullException(nameof(breaksController));
		}

		public async Task Consume(ConsumeContext<OperatorState> context)
		{
			switch(context.Message.Trigger)
			{
				case OperatorTrigger.StartBreak:
				case OperatorTrigger.EndBreak:
					_breaksController.UpdateBreakAvailability();
					break;
				case OperatorTrigger.Connect:
				case OperatorTrigger.StartWorkShift:
				case OperatorTrigger.TakeCall:
				case OperatorTrigger.EndCall:
				case OperatorTrigger.ChangePhone:
				case OperatorTrigger.EndWorkShift:
				case OperatorTrigger.Disconnect:
				default:
					break;
			}

			await Task.CompletedTask;
		}
	}

	public class OperatorStateConsumerDefinition : ConsumerDefinition<OperatorStateConsumer>
	{
		public OperatorStateConsumerDefinition()
		{
			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.operator_state.consumer-break-server";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OperatorStateConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OperatorState>(c =>
				{
					c.RoutingKey = $"pacs.operator.state.#";
				});
			}
		}
	}
}
