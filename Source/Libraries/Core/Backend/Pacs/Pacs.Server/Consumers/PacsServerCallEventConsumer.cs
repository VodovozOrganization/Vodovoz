using MassTransit;
using Pacs.Core.Messages.Events;
using Pacs.Server.Operators;
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
}
