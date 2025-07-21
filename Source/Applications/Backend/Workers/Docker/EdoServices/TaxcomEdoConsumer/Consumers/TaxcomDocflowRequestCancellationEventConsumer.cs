using Edo.Docflow.Taxcom;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace TaxcomEdoConsumer.Consumers
{
	public class TaxcomDocflowRequestCancellationEventConsumer : IConsumer<TaxcomDocflowRequestCancellationEvent>
	{
		private readonly ILogger<TaxcomDocflowSendEventConsumer> _logger;
		private readonly IEdoDocflowHandler _edoDocflowHandler;

		public TaxcomDocflowRequestCancellationEventConsumer(
			ILogger<TaxcomDocflowSendEventConsumer> logger,
			IEdoDocflowHandler edoDocflowHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoDocflowHandler = edoDocflowHandler ?? throw new ArgumentNullException(nameof(edoDocflowHandler));
		}

		public async Task Consume(ConsumeContext<TaxcomDocflowRequestCancellationEvent> context)
		{
			var message = context.Message;

			try
			{
				_logger.LogInformation(
					"Создаем предложение об аннулировании документа {DocumentId}",
					message.DocumentId
				);

				await _edoDocflowHandler.SendOfferCancellation(message, context.CancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при создании предложения об аннулировании документа {DocumentId}",
					message.DocumentId);
			}
		}
	}
}
