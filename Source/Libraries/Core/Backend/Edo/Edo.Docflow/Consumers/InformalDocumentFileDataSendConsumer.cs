using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Docflow.Consumers
{
	/// <summary>
	/// Консьюмер события отправки файловых данных неформализованного документа
	/// </summary>
	public class InformalDocumentFileDataSendConsumer : IConsumer<InformalDocumentFileDataSendEvent>
	{
		private readonly ILogger<InformalDocumentFileDataSendConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public InformalDocumentFileDataSendConsumer(
			ILogger<InformalDocumentFileDataSendConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<InformalDocumentFileDataSendEvent> context)
		{
			await _docflowHandler.HandleInformalOrderDocument(context.Message.DocumentId, context.Message.FileData, context.CancellationToken);
		}
	}
}
