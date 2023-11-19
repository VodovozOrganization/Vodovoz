using MassTransit;
using Microsoft.Extensions.Options;
using Pacs.Core.Messages.Commands;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace Pacs.Server.Consumers
{
	public class OperatorConnectConsumer : IConsumer<Connect>
	{
		private readonly IOperatorControllerProvider _controllerProvider;

		public OperatorConnectConsumer(IOperatorControllerProvider controllerProvider)
		{
			_controllerProvider = controllerProvider ?? throw new System.ArgumentNullException(nameof(controllerProvider));
		}

		public async Task Consume(ConsumeContext<Connect> context)
		{
			var message = context.Message;

			var controller = _controllerProvider.GetOperatorController(message.OperatorId);
			var result = await controller.Connect();

			await context.RespondAsync(result);
		}
	}

	public class ContentReceivedConsumerDefinition : ConsumerDefinition<OperatorConnectConsumer>
	{
		public ContentReceivedConsumerDefinition()
		{
			EndpointName = $"pacs.operator.service.connect";
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OperatorConnectConsumer> consumerConfigurator)
		{
			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;

				rmq.Bind<Connect>();
			}
		}
	}
}
