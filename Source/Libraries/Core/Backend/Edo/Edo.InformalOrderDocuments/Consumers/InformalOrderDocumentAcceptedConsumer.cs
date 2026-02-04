using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Handlers;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.InformalOrderDocuments.Consumers
{
	/// <summary>
	/// Потребитель события принятия документа
	/// </summary>
	public class InformalOrderDocumentAcceptedConsumer : IConsumer<InformalOrderDocumentAcceptedEvent>
	{
		private readonly OrderDocumentEdoTaskHandler _orderDocumentEdoTaskHandler;
		private readonly ILogger<InformalOrderDocumentAcceptedConsumer> _logger;

		public InformalOrderDocumentAcceptedConsumer(
			OrderDocumentEdoTaskHandler orderDocumentEdoTaskHandler,
			ILogger<InformalOrderDocumentAcceptedConsumer> logger
			)
		{
			_orderDocumentEdoTaskHandler = orderDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(orderDocumentEdoTaskHandler));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<InformalOrderDocumentAcceptedEvent> context)
		{
			try
			{
				_logger.LogInformation($"Получено событие {nameof(InformalOrderDocumentAcceptedEvent)}, DocumentId: {context.Message.DocumentId}");
				await _orderDocumentEdoTaskHandler.HandleAccepted(context.Message.DocumentId, context.CancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Обнаружена ошибка при обработке события {nameof(InformalOrderDocumentAcceptedEvent)}, DocumentId: {context.Message.DocumentId}");
				throw;
			}
		}
	}
}
