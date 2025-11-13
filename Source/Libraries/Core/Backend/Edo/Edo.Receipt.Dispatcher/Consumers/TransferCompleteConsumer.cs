using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Receipt.Dispatcher.Consumers
{
	public class TransferCompleteConsumer : IConsumer<TransferCompleteEvent>
	{
		private readonly ReceiptEdoTaskHandler _receiptEdoTaskHandler;

		public TransferCompleteConsumer(ReceiptEdoTaskHandler receiptEdoTaskHandler)
		{
			_receiptEdoTaskHandler = receiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(receiptEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<TransferCompleteEvent> context)
		{
			if(context.Message.TransferInitiator != TransferInitiator.Receipt)
			{
				throw new InvalidOperationException("Не правильно настроена маршрутизация для сообщения " +
					$"{nameof(TransferCompleteEvent)}. Получено сообщение для {context.Message.TransferInitiator}, " +
					$"а должно быть для {nameof(TransferInitiator.Receipt)}");
			}
			await _receiptEdoTaskHandler.HandleTransfered(context.Message.TransferIterationId, context.CancellationToken);
		}
	}
}

