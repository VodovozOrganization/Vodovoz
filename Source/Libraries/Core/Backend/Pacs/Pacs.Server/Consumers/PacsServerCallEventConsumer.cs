using Core.Infrastructure;
using MassTransit;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using CallState = Vodovoz.Core.Domain.Pacs.CallState;

namespace Pacs.Server.Consumers
{
	public class PacsServerCallEventConsumer : IConsumer<CallEvent>
	{
		private readonly IOperatorControllerProvider _operatorControllerProvider;

		public PacsServerCallEventConsumer(IOperatorControllerProvider operatorControllerProvider)
		{
			_operatorControllerProvider = operatorControllerProvider ?? throw new ArgumentNullException(nameof(operatorControllerProvider));
		}

		public async Task Consume(ConsumeContext<CallEvent> context)
		{
			if(string.IsNullOrWhiteSpace(context.Message.ToExtension))
			{
				return;
			}

			var operatorController = _operatorControllerProvider.GetOperatorController(context.Message.ToExtension);
			if(operatorController == null)
			{
				return;
			}

			switch(context.Message.CallState)
			{
				case CallState.Connected:
					await operatorController.TakeCall(context.Message.CallId);
					break;
				case CallState.Disconnected:
					await operatorController.EndCall(context.Message.CallId);
					break;
				case CallState.Appeared:
				case CallState.OnHold:
				default:
						break;
			}
		}
	}

	public class PacsServerCallEventConsumerDefinition : ConsumerDefinition<PacsServerCallEventConsumer>
	{
		public PacsServerCallEventConsumerDefinition()
		{
			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.call.consumer-server";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<PacsServerCallEventConsumer> consumerConfigurator)
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
