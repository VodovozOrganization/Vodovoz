using MassTransit;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using System;
using System.Threading.Tasks;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Consumer сообщения об отправке контрагента в Mindbox
	/// </summary>
	public sealed class MindboxCounterpartySendConsumer : IConsumer<MindboxCounterpartySendMessage>
	{
		private readonly IMindboxCounterpartySendService _sendService;
		private readonly ILogger<MindboxCounterpartySendConsumer> _logger;

		public MindboxCounterpartySendConsumer(
			IMindboxCounterpartySendService sendService,
			ILogger<MindboxCounterpartySendConsumer> logger)
		{
			_sendService = sendService ?? throw new ArgumentNullException(nameof(sendService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task Consume(ConsumeContext<MindboxCounterpartySendMessage> context)
		{
			try
			{
				await _sendService.SendAsync(
					context.Message.CounterpartyId,
					context.Message.TransactionId,
					context.GetRetryAttempt(),
					context.CancellationToken)
					.ConfigureAwait(false);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка отправки контрагента {CounterpartyId} в Mindbox, transactionId={TransactionId}",
					context.Message.CounterpartyId,
					context.Message.TransactionId);

				throw;
			}
		}
	}
}
