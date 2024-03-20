using Mango.Core.Dto;
using MassTransit;
using Pacs.MangoCalls.Services;
using System;
using System.Threading.Tasks;

namespace Pacs.MangoCalls.Consumers
{
	public class MangoSummaryEventConsumer : IConsumer<MangoSummaryEvent>
	{
		private readonly ICallEventRegistrar _callEventRegistrar;

		public MangoSummaryEventConsumer(ICallEventRegistrar callEventRegistrar)
		{
			_callEventRegistrar = callEventRegistrar ?? throw new ArgumentNullException(nameof(callEventRegistrar));
		}

		public async Task Consume(ConsumeContext<MangoSummaryEvent> context)
		{
			await _callEventRegistrar.RegisterSummaryEvent(context.Message);
		}
	}
}
