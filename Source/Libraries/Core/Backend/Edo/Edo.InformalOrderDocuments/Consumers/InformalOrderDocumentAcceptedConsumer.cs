using Edo.Contracts.Messages.Events;
using Edo.InformalOrderDocuments.Handlers;
using MassTransit;
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

		public InformalOrderDocumentAcceptedConsumer(OrderDocumentEdoTaskHandler orderDocumentEdoTaskHandler)
		{
			_orderDocumentEdoTaskHandler = orderDocumentEdoTaskHandler ?? throw new ArgumentNullException(nameof(orderDocumentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<InformalOrderDocumentAcceptedEvent> context)
		{
			await _orderDocumentEdoTaskHandler.HandleAccepted(context.Message.DocumentId, context.CancellationToken);
		}
	}
}
