using Edo.Contracts.Messages.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Admin.Consumers
{
	public class OrderDocumentCancelledConsumer : IConsumer<OrderDocumentCancelledEvent>
	{
		private readonly EdoCancellationService _edoCancellationService;

		public OrderDocumentCancelledConsumer(EdoCancellationService edoCancellationService)
		{
			_edoCancellationService = edoCancellationService ?? throw new ArgumentNullException(nameof(edoCancellationService));
		}

		public async Task Consume(ConsumeContext<OrderDocumentCancelledEvent> context)
		{
			var message = context.Message;
			await _edoCancellationService.AcceptOrderTaskCancellation(message.DocumentId, context.CancellationToken);
		}
	}
}
