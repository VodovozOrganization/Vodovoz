using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Docflow.Consumers
{
	public class TransferDocumentSendConsumer : IConsumer<TransferDocumentSendEvent>
	{
		private readonly ILogger<TransferDocumentSendConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public TransferDocumentSendConsumer(
			ILogger<TransferDocumentSendConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<TransferDocumentSendEvent> context)
		{
			try
			{
				await _docflowHandler.HandleTransferDocument(context.Message.Id, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке события отправки трансфер документа");
			}
		}
	}
}
