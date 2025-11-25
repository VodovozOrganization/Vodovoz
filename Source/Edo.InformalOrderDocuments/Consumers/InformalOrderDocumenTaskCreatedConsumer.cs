using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Handlers;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.InformalOrderDocuments.Consumers
{
	/// <summary>
	/// Настройка MassTransit для обработки созданной задачи документу заказа
	/// </summary>
	public class InformalOrderDocumenTaskCreatedConsumer : IConsumer<InformalOrderDocumenTaskCreatedEvent>
	{
		private readonly ILogger<InformalOrderDocumenTaskCreatedConsumer> _logger;
		private readonly OrderDocumentEdoTaskHandler _orderDocumentEdoTaskHandler;

		public InformalOrderDocumenTaskCreatedConsumer(
			ILogger<InformalOrderDocumenTaskCreatedConsumer> logger,
			OrderDocumentEdoTaskHandler orderDocumentEdoTaskHandler
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_orderDocumentEdoTaskHandler = orderDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(orderDocumentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<InformalOrderDocumenTaskCreatedEvent> context)
		{
			await _orderDocumentEdoTaskHandler.HandleNew(context.Message.InformalOrderDocumentTaskId, context.CancellationToken);
		}
	}
}
