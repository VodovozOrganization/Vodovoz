using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Consumer синхронизации онлайн-заказа с Mindbox
	/// </summary>
	public sealed class MindboxOnlineOrderSyncConsumer : IConsumer<MindboxOnlineOrderSyncMessage>
	{
		private readonly IMindboxOnlineOrderSyncProcessor _processor;
		private readonly ILogger<MindboxOnlineOrderSyncConsumer> _logger;

		public MindboxOnlineOrderSyncConsumer(
			IMindboxOnlineOrderSyncProcessor processor,
			ILogger<MindboxOnlineOrderSyncConsumer> logger)
		{
			_processor = processor ?? throw new ArgumentNullException(nameof(processor));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Обрабатывает сообщение синхронизации онлайн-заказа
		/// </summary>
		/// <param name="context">Контекст сообщения MassTransit</param>
		/// <returns>Задача обработки сообщения</returns>
		public async Task Consume(ConsumeContext<MindboxOnlineOrderSyncMessage> context)
		{
			try
			{
				await _processor.SyncAsync(
					context.Message.OnlineOrderId,
					context.Message.TransactionId,
					context.CancellationToken).ConfigureAwait(false);
			}
			catch(MindboxTransientException)
			{
				throw;
			}
			catch(OperationCanceledException) when(context.CancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка синхронизации онлайн-заказа {OnlineOrderId} с Mindbox", context.Message.OnlineOrderId);
				throw;
			}
		}
	}
}
