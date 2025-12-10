using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Handlers;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.InformalOrderDocuments.Consumers
{
	/// <summary>
	/// Потребитель события проблемы с документом заказа
	/// </summary>
	public class InformalOrderDocumentProblemConsumer : IConsumer<InformalOrderDocumentProblemEvent>
	{
		private readonly OrderDocumentEdoTaskHandler _orderDocumentEdoTaskHandler;
		private readonly ILogger<InformalOrderDocumentProblemConsumer> _logger;

		public InformalOrderDocumentProblemConsumer(
			OrderDocumentEdoTaskHandler orderDocumentEdoTaskHandler,
			ILogger<InformalOrderDocumentProblemConsumer> logger
			)
		{
			_orderDocumentEdoTaskHandler = orderDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(orderDocumentEdoTaskHandler));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<InformalOrderDocumentProblemEvent> context)
		{
			try
			{
				_logger.LogInformation(
					$"Получено событие {nameof(InformalOrderDocumentProblemEvent)}, DocumentId: {context.Message.DocumentId}");
				await _orderDocumentEdoTaskHandler.HandleProblem(context.Message.DocumentId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex, $"Обнаружена ошибка при обработке события {nameof(InformalOrderDocumentProblemEvent)}, DocumentId: {context.Message.DocumentId}");
				throw;
			}
		}
	}
}
