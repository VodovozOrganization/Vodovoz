using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Handlers;
using MassTransit;
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

		public InformalOrderDocumentCancelledConsumer(OrderDocumentEdoTaskHandler orderDocumentEdoTaskHandler)
		{
			_orderDocumentEdoTaskHandler = orderDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(orderDocumentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<InformalOrderDocumentCancelledEvent> context)
		{
			await _orderDocumentEdoTaskHandler.HandleCancelled(context.Message.DocumentId, context.CancellationToken);
		}
	}
}
