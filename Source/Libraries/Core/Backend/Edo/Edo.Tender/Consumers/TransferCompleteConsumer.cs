using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Tender.Consumers
{
	public class TransferCompleteConsumer : IConsumer<TransferCompleteEvent>
	{
		private readonly ILogger<TransferCompleteConsumer> _logger;
		private readonly TenderEdoTaskHandler _tenderEdoTaskHandler;

		public TransferCompleteConsumer(
			ILogger<TransferCompleteConsumer> logger,
			TenderEdoTaskHandler documentEdoTaskHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_tenderEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<TransferCompleteEvent> context)
		{
			if(context.Message.TransferInitiator != TransferInitiator.Tender)
			{
				throw new InvalidOperationException("Не правильно настроена маршрутизация для сообщения " +
				                                    $"{nameof(TransferCompleteEvent)}. Получено сообщение для {context.Message.TransferInitiator}, " +
				                                    $"а должно быть для {nameof(TransferInitiator.Tender)}");
			}

			await _tenderEdoTaskHandler.HandleTransfered(context.Message.TransferIterationId, context.CancellationToken);
		}
	}
}
