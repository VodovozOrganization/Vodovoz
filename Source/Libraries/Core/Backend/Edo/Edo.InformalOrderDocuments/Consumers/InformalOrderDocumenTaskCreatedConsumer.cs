using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Handlers;
using MassTransit;
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

		public InformalOrderDocumenTaskCreatedConsumer(
			OrderDocumentEdoTaskHandler orderDocumentEdoTaskHandler
		)
		{
			_orderDocumentEdoTaskHandler = orderDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(orderDocumentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<InformalOrderDocumenTaskCreatedEvent> context)
		{
			await _orderDocumentEdoTaskHandler.HandleNew(context.Message.InformalOrderDocumentTaskId, context.CancellationToken);
		}
	}
}
