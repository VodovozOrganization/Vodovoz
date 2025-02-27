using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Edo.Docflow.Consumers
{
	public class CustomerDocumentSendConsumer : IConsumer<CustomerDocumentSendEvent>
	{
		private readonly ILogger<CustomerDocumentSendConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public CustomerDocumentSendConsumer(
			ILogger<CustomerDocumentSendConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<CustomerDocumentSendEvent> context)
		{
			try
			{
				await _docflowHandler.HandleCustomerDocument(context.Message.CustomerDocumentId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке события отправки клиентского документа");
			}
		}
	}
}
