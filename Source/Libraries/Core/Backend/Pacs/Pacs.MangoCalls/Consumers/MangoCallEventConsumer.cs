using Mango.Core.Dto;
using MassTransit;
using Pacs.MangoCalls.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pacs.MangoCalls.Consumers
{
	public class MangoCallEventConsumer : IConsumer<Batch<MangoCallEvent>>
	{
		private readonly ICallEventRegistrar _callEventRegistrar;

		public MangoCallEventConsumer(ICallEventRegistrar callEventRegistrar)
		{
			_callEventRegistrar = callEventRegistrar ?? throw new ArgumentNullException(nameof(callEventRegistrar));
		}

		public async Task Consume(ConsumeContext<Batch<MangoCallEvent>> context)
		{
			var messages = context.Message.Select(x => x.Message);
			await _callEventRegistrar.RegisterCallEvents(messages);
		}
	}
}
