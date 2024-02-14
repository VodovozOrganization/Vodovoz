using Mango.Core.Dto;
using MassTransit;
using Pacs.MangoCalls.Services;
using System;
using System.Threading.Tasks;

namespace Pacs.MangoCalls.Consumers
{
	public class MangoCallEventConsumer : IConsumer<MangoCallEvent>
	{
		private readonly ICallEventRegistrar _callEventRegistrar;

		public MangoCallEventConsumer(ICallEventRegistrar callEventRegistrar)
		{
			_callEventRegistrar = callEventRegistrar ?? throw new ArgumentNullException(nameof(callEventRegistrar));
		}

		public async Task Consume(ConsumeContext<MangoCallEvent> context)
		{
			var callEvent = context.Message;
			await _callEventRegistrar.RegisterCallEvent(callEvent);
			return;
		}
	}
}
