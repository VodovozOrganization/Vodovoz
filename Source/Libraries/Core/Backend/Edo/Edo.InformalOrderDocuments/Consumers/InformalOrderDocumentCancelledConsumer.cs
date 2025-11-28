using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Handlers;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.InformalOrderDocuments.Consumers
{
	/// <summary>
	/// Потребитель события аннулирования документа
	/// </summary>
	public class InformalOrderDocumentCancelledConsumer : IConsumer<InformalOrderDocumentCancelledEvent>
	{
		private readonly OrderDocumentEdoTaskHandler _orderDocumentEdoTaskHandler;
		private readonly ILogger<InformalOrderDocumentCancelledConsumer> _logger;

		public InformalOrderDocumentCancelledConsumer(
			OrderDocumentEdoTaskHandler orderDocumentEdoTaskHandler,
			ILogger<InformalOrderDocumentCancelledConsumer> logger
			)
		{
			_orderDocumentEdoTaskHandler = orderDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(orderDocumentEdoTaskHandler));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<InformalOrderDocumentCancelledEvent> context)
		{
			try
			{
				_logger.LogInformation(
					$"Получено событие {nameof(InformalOrderDocumentCancelledEvent)}, DocumentId: {context.Message.DocumentId}");
				await _orderDocumentEdoTaskHandler.HandleCancelled(context.Message.DocumentId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex, $"Обнаружена ошибка при обработке события {nameof(InformalOrderDocumentCancelledEvent)}, DocumentId: {context.Message.DocumentId}");
				throw;
			}
		}
	}
}
