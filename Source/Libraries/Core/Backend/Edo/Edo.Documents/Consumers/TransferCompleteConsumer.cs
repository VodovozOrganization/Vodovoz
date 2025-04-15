using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents.Consumers
{
	public class TransferCompleteConsumer : IConsumer<TransferCompleteEvent>
	{
		private readonly ILogger<TransferCompleteConsumer> _logger;
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public TransferCompleteConsumer(
			ILogger<TransferCompleteConsumer> logger,
			DocumentEdoTaskHandler documentEdoTaskHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<TransferCompleteEvent> context)
		{
			if(context.Message.TransferInitiator != TransferInitiator.Document)
			{
				throw new InvalidOperationException("Не правильно настроена маршрутизация для сообщения " +
					$"{nameof(TransferCompleteEvent)}. Получено сообщение для {context.Message.TransferInitiator}, " +
					$"а должно быть для {nameof(TransferInitiator.Document)}");
			}

			await _documentEdoTaskHandler.HandleTransfered(context.Message.TransferIterationId, context.CancellationToken);
		}
	}
}

