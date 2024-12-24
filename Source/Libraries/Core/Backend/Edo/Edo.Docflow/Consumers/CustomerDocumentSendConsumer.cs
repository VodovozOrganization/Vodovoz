using Edo.Transport.Messages.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Docflow.Consumers
{
	public class CustomerDocumentSendConsumer : IConsumer<CustomerDocumentSendEvent>
	{
		private readonly DocflowHandler _docflowHandler;

		public CustomerDocumentSendConsumer(DocflowHandler docflowHandler)
		{
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<CustomerDocumentSendEvent> context)
		{
			await _docflowHandler.HandleCustomerDocument(context.Message.Id, context.CancellationToken);
		}
	}
}
