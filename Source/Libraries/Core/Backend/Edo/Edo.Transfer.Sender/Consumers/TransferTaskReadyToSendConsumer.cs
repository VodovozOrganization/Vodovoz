using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Edo.Transfer.Sender.Consumers
{
	public class TransferTaskReadyToSendConsumer : IConsumer<TransferTaskReadyToSendEvent>
	{
		private readonly ILogger<TransferTaskReadyToSendConsumer> _logger;
		private readonly TransferSendHandler _transferSendHandler;

		public TransferTaskReadyToSendConsumer(
			ILogger<TransferTaskReadyToSendConsumer> logger,
			TransferSendHandler transferSendHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_transferSendHandler = transferSendHandler ?? throw new ArgumentNullException(nameof(transferSendHandler));
		}

		public async Task Consume(ConsumeContext<TransferTaskReadyToSendEvent> context)
		{
			try
			{
				await _transferSendHandler.HandleReadyToSend(context.Message.Id, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке события готовности к отправке задачи переноса кодов.");
			}
		}
	}
}

