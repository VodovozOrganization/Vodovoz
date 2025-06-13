using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Admin.Consumers
{
	public class TransferDocumentCancelledConsumer : IConsumer<TransferDocumentCancelledEvent>
	{
		private readonly ILogger<EdoCancellationConsumer> _logger;
		private readonly EdoCancellationService _edoCancellationService;

		public TransferDocumentCancelledConsumer(
			ILogger<EdoCancellationConsumer> logger,
			EdoCancellationService edoCancellationService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoCancellationService = edoCancellationService ?? throw new ArgumentNullException(nameof(edoCancellationService));
		}

		public async Task Consume(ConsumeContext<TransferDocumentCancelledEvent> context)
		{
			var message = context.Message;
			await _edoCancellationService.AcceptTransferTaskCancellation(message.DocumentId, context.CancellationToken);
		}
	}
}
