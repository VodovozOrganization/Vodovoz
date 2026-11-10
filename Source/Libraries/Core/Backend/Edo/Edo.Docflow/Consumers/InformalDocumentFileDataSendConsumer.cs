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
		private readonly DocflowSendHandler _docflowHandler;

		public InformalDocumentFileDataSendConsumer(
			ILogger<InformalDocumentFileDataSendConsumer> logger,
			DocflowSendHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<InformalDocumentFileDataSendEvent> context)
		{
			var message = context.Message;
			try
			{
				_logger.LogInformation(
					"Обрабатываем событие {EventName}, OrderId: {OrderId}",
					nameof(InformalDocumentFileDataSendEvent),
					message.FileData.OrderId);

				await _docflowHandler.SendInformalOrderDocument(message.DocumentId, message.FileData, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при обработке события {EventName}, OrderId: {OrderId}",
					nameof(InformalDocumentFileDataSendEvent),
					message.FileData.OrderId);
				throw;
			}
		}
	}
}
