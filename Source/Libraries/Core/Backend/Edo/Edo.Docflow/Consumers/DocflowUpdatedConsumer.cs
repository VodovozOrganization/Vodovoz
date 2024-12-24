using Edo.Transport.Messages.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Docflow.Consumers
{
	public class DocflowUpdatedConsumer : IConsumer<EdoDocflowUpdatedEvent>
	{
		private readonly DocflowHandler _docflowHandler;

		public DocflowUpdatedConsumer(DocflowHandler docflowHandler)
		{
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<EdoDocflowUpdatedEvent> context)
		{
			// необходимо написать процесс обновления документооборота по событию из такском сервисов
			await _docflowHandler.HandleCustomerDocument(context.Message, context.CancellationToken);
		}
	}
}
