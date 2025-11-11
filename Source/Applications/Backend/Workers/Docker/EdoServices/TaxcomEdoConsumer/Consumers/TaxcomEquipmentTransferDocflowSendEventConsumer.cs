using Edo.Docflow.Taxcom;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace TaxcomEdoConsumer.Consumers
{
	public class TaxcomEquipmentTransferDocflowSendEventConsumer : IConsumer<TaxcomDocflowEquipmentTransferSendEvent>
	{
		private readonly ILogger<TaxcomEquipmentTransferDocflowSendEventConsumer> _logger;
		private readonly IEdoDocflowHandler _edoDocflowHandler;

		public TaxcomEquipmentTransferDocflowSendEventConsumer(
			ILogger<TaxcomEquipmentTransferDocflowSendEventConsumer> logger,
			IEdoDocflowHandler edoDocflowHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoDocflowHandler = edoDocflowHandler ?? throw new ArgumentNullException(nameof(edoDocflowHandler));
		}

		public async Task Consume(ConsumeContext<TaxcomDocflowEquipmentTransferSendEvent> context)
		{
			var message = context.Message;

			try
			{
				_logger.LogInformation(
					"Создаем исходящий документооборот с неформализованным документом типа {DocumentType}, {EdoDocument}",
					message.DocumentType,
					message.DocumentInfo.MainDocumentId);

				await _edoDocflowHandler.CreateTaxcomDocFlowAndSendEquipmentTransferDocument(message);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при создании исходящего документооборота с неформализованным документом типа {DocumentType}, {EdoDocument}",
					message.DocumentType,
					message.DocumentInfo.MainDocumentId);
			}
		}
	}
}

