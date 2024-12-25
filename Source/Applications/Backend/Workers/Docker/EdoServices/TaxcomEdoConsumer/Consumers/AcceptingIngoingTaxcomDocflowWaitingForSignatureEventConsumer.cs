using Edo.Docflow.Taxcom;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace TaxcomEdoConsumer.Consumers
{
	public class AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer
		: IConsumer<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent>
	{
		private readonly ILogger<AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer> _logger;
		private readonly IEdoDocflowHandler _edoDocflowHandler;

		public AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer(
			ILogger<AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer> logger,
			IEdoDocflowHandler edoDocflowHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoDocflowHandler = edoDocflowHandler ?? throw new ArgumentNullException(nameof(edoDocflowHandler));
		}

		public async Task Consume(ConsumeContext<AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent> context)
		{
			var message = context.Message;

			try
			{
				_logger.LogInformation(
					"Отправляем информацию о требующем подписания входящем документе {EdoDocument}",
					message.MainDocumentId);

				await _edoDocflowHandler.AcceptIngoingTaxcomEdoDocFlowWaitingForSignature(message);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при отправке информации о требующем подписания входящем документе {EdoDocument}",
					message.MainDocumentId);
			}
		}
	}
}
