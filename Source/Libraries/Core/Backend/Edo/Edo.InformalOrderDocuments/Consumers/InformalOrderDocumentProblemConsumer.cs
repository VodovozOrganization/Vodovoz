using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Handlers;
using MassTransit;
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

		public InformalOrderDocumentProblemConsumer(OrderDocumentEdoTaskHandler orderDocumentEdoTaskHandler)
		{
			_orderDocumentEdoTaskHandler = orderDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(orderDocumentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<InformalOrderDocumentProblemEvent> context)
		{
			await _orderDocumentEdoTaskHandler.HandleProblem(context.Message.DocumentId, context.CancellationToken);
		}
	}
}
