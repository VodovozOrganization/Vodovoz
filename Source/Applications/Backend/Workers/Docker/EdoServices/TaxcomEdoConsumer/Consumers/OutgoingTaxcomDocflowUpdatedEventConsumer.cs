using System;
using System.Threading.Tasks;
using Edo.Docflow.Taxcom;
using Edo.Transport2;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace TaxcomEdoConsumer.Consumers
{
	public class OutgoingTaxcomDocflowUpdatedEventConsumer : IConsumer<OutgoingTaxcomDocflowUpdatedEvent>
	{
		private readonly ILogger<OutgoingTaxcomDocflowUpdatedEventConsumer> _logger;
		private readonly IEdoDocflowHandler _edoDocflowHandler;

		public OutgoingTaxcomDocflowUpdatedEventConsumer(
			ILogger<OutgoingTaxcomDocflowUpdatedEventConsumer> logger,
			IEdoDocflowHandler edoDocflowHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoDocflowHandler = edoDocflowHandler ?? throw new ArgumentNullException(nameof(edoDocflowHandler));
		}

		public async Task Consume(ConsumeContext<OutgoingTaxcomDocflowUpdatedEvent> context)
		{
			var message = context.Message;

			try
			{
				_logger.LogInformation(
					"Отправляем информацию об обновленном исходящем документообороте с документом {EdoDocument}",
					message.MainDocumentId);

				await _edoDocflowHandler.UpdateOutgoingTaxcomDocFlow(message);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при отправке информации об обновленном исходящем документообороте с документом {EdoDocument}",
					message.MainDocumentId);
			}
		}
	}
}
