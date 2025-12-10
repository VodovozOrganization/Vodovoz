using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Handlers;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.InformalOrderDocuments.Consumers
{
	/// <summary>
	/// Консьюмер для обработки созданной задачи документу заказа
	/// </summary>
	public class InformalOrderDocumenTaskCreatedConsumer : IConsumer<InformalOrderDocumenTaskCreatedEvent>
	{
		private readonly OrderDocumentEdoTaskHandler _orderDocumentEdoTaskHandler;
		private readonly ILogger<InformalOrderDocumenTaskCreatedConsumer> _logger;

		public InformalOrderDocumenTaskCreatedConsumer(
			OrderDocumentEdoTaskHandler orderDocumentEdoTaskHandler,
			ILogger<InformalOrderDocumenTaskCreatedConsumer> logger
		)
		{
			_orderDocumentEdoTaskHandler = orderDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(orderDocumentEdoTaskHandler));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<InformalOrderDocumenTaskCreatedEvent> context)
		{
			try
			{
				_logger.LogInformation(
					$"Получено событие {nameof(InformalOrderDocumenTaskCreatedEvent)}, DocumentId: {context.Message.InformalOrderDocumentTaskId}");
				await _orderDocumentEdoTaskHandler.HandleNew(context.Message.InformalOrderDocumentTaskId, context.CancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex, $"Обнаружена ошибка при обработке события {nameof(InformalOrderDocumenTaskCreatedEvent)}, DocumentId: {context.Message.InformalOrderDocumentTaskId}");
				throw;
			}
		}
	}
}
