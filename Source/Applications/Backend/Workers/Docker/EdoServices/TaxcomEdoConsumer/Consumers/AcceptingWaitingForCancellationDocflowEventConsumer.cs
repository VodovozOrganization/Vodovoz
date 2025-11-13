using Edo.Docflow.Taxcom;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace TaxcomEdoConsumer.Consumers
{
	public class AcceptingWaitingForCancellationDocflowEventConsumer
		: IConsumer<AcceptingWaitingForCancellationDocflowEvent>
	{
		private readonly ILogger<AcceptingWaitingForCancellationDocflowEventConsumer> _logger;
		private readonly IEdoDocflowHandler _edoDocflowHandler;

		public AcceptingWaitingForCancellationDocflowEventConsumer(
			ILogger<AcceptingWaitingForCancellationDocflowEventConsumer> logger,
			IEdoDocflowHandler edoDocflowHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoDocflowHandler = edoDocflowHandler ?? throw new ArgumentNullException(nameof(edoDocflowHandler));
		}

		public async Task Consume(ConsumeContext<AcceptingWaitingForCancellationDocflowEvent> context)
		{
			var message = context.Message;

			try
			{
				_logger.LogInformation(
					"Отправляем подтверждение предложения об аннулировании документа {DocflowId}",
					message.DocFlowId
				);

				await _edoDocflowHandler.AcceptOfferCancellation(message, context.CancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при подтверждении предложения об аннулировании документа {DocflowId}",
					message.DocFlowId
				);
			}
		}
	}
}
