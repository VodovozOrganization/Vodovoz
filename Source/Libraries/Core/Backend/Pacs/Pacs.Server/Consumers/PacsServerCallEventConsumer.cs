using MassTransit;
using Pacs.Core.Messages.Events;
using Pacs.Server.Operators;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Consumers
{
	public class PacsServerCallEventConsumer : IConsumer<PacsCallEvent>
	{
		private readonly IOperatorStateService _operatorStateService;

		public PacsServerCallEventConsumer(IOperatorStateService operatorControllerProvider)
		{
			_operatorStateService = operatorControllerProvider ?? throw new ArgumentNullException(nameof(operatorControllerProvider));
		}

		public async Task Consume(ConsumeContext<PacsCallEvent> context)
		{
			var call = context.Message.Call;

			if(call.CallDirection.HasValue && call.CallDirection != CallDirection.Incoming)
			{
				return;
			}

			var connectedSubCall = call.SubCalls.FirstOrDefault(x => x.WasConnected);

			if(connectedSubCall == null)
			{
				return;
			}

			if(call.Status == CallStatus.Connected)
			{
				await _operatorStateService.TakeCall(connectedSubCall.ToExtension, connectedSubCall.CallId);
			}
			else
			{
				await _operatorStateService.EndCall(connectedSubCall.ToExtension, connectedSubCall.CallId);
			}
		}
	}
}
