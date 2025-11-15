using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Docflow.Consumers
{
	public class EquipmentTransferDocumentSendConsumer : IConsumer<EquipmentTransferDocumentSendEvent>
	{
		private readonly ILogger<OrderDocumentSendConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public EquipmentTransferDocumentSendConsumer(
			ILogger<OrderDocumentSendConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<EquipmentTransferDocumentSendEvent> context)
		{
			await _docflowHandler.HandleEquipmentTransferDocument(context.Message.EquipmentTransferDocumentId, context.CancellationToken);
		}
	}
}
