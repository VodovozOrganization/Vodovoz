using Edo.Docflow.Taxcom;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace TaxcomEdoConsumer.Consumers
{
	public class OutgoingTaxcomDocflowUpdatedEventConsumer : IConsumer<OutgoingTaxcomDocflowUpdatedEvent>
	{
		private readonly ILogger<OutgoingTaxcomDocflowUpdatedEventConsumer> _logger;
		private readonly IEdoDocflowHandler _edoDocflowHandler;
		private readonly IPublishEndpoint _publishEndpoint;

		public OutgoingTaxcomDocflowUpdatedEventConsumer(
			ILogger<OutgoingTaxcomDocflowUpdatedEventConsumer> logger,
			IEdoDocflowHandler edoDocflowHandler,
			IPublishEndpoint publishEndpoint)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoDocflowHandler = edoDocflowHandler ?? throw new ArgumentNullException(nameof(edoDocflowHandler));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		public async Task Consume(ConsumeContext<OutgoingTaxcomDocflowUpdatedEvent> context)
		{
			var message = context.Message;

			try
			{
				_logger.LogInformation(
					"Обрабатываем информацию об обновленном исходящем документообороте {DocflowId} с документом {EdoDocument}",
					message.DocFlowId,
					message.MainDocumentId
					);

				var edoDocflowUpdatedEvent = await _edoDocflowHandler.UpdateOutgoingTaxcomDocFlow(message);

				if(edoDocflowUpdatedEvent != null)
				{
					_logger.LogInformation(
						"Отправляем информацию об обновленном исходящем документообороте {DocflowId} с документом {EdoDocument}",
						message.DocFlowId,
						message.MainDocumentId
					);
					await _publishEndpoint.Publish(edoDocflowUpdatedEvent);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при отправке информации об обновленном исходящем документообороте {DocflowId} с документом {EdoDocument}",
					message.DocFlowId,
					message.MainDocumentId);
			}
		}
	}
}
