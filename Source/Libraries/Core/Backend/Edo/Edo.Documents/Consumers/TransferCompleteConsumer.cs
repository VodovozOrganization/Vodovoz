using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents.Consumers
{
	public class TransferCompleteConsumer : IConsumer<TransferCompleteEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public TransferCompleteConsumer(DocumentEdoTaskHandler documentEdoTaskHandler)
		{
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

