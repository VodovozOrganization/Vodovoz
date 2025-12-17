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
			try
			{
				_logger.LogInformation(
					$"Получено событие {nameof(InformalDocumentFileDataSendEvent)}, DocumentId: {context.Message.FileData}");

				await _docflowHandler.HandleInformalOrderDocument(context.Message.DocumentId, context.Message.FileData, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex, $"Обнаружена ошибка при обработке события {nameof(InformalDocumentFileDataSendEvent)}, DocumentId: {context.Message.FileData}");
				throw;
			}
		}
	}
}
