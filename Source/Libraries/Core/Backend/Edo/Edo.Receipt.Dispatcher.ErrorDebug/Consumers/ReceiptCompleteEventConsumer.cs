using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class ReceiptCompleteEventConsumer : IConsumer<ReceiptCompleteEvent>
	{
		private readonly ILogger<ReceiptTaskCreatedEventConsumer> _logger;
		private readonly ReceiptEdoTaskHandler _receiptEdoTaskHandler;

		public ReceiptCompleteEventConsumer(
			ILogger<ReceiptTaskCreatedEventConsumer> logger,
			ReceiptEdoTaskHandler receiptEdoTaskHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_receiptEdoTaskHandler = receiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(receiptEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<ReceiptCompleteEvent> context)
		{
			// сообщение, вызвавшее ошибку
			//var message = context.Message.Message;
			var message = context.Message;

			// исключение, вызвавшее ошибку
			//var exception = context.Message.Exceptions.FirstOrDefault();

			try
			{
				await _receiptEdoTaskHandler.HandleCompleted(context.Message.ReceiptEdoTaskId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error while processing EdoRequestCreatedEvent");
				await Task.CompletedTask;
			}
		}
	}
}
