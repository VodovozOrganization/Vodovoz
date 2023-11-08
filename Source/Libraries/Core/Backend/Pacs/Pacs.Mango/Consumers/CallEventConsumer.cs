using Mango.Core.Dto;
using MassTransit;
using Pacs.Mango.Services;
using System;
using System.Threading.Tasks;

namespace Pacs.Mango.Consumers
{
	public class CallEventConsumer : IConsumer<CallEvent>
	{
		private readonly ICallEventSaver _callEventSaver;

		public CallEventConsumer(ICallEventSaver callEventSaver)
		{
			_callEventSaver = callEventSaver ?? throw new ArgumentNullException(nameof(callEventSaver));
		}

		public async Task Consume(ConsumeContext<CallEvent> context)
		{
			var callEvent = context.Message;
			await _callEventSaver.SaveCallEvent(callEvent);
			return;
		}
	}
}
