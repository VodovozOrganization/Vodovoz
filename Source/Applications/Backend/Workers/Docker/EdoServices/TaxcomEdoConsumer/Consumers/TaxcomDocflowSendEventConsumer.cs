using Edo.Docflow.Taxcom;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace TaxcomEdoConsumer.Consumers
{
	public class TaxcomDocflowSendEventConsumer : IConsumer<TaxcomDocflowSendEvent>
	{
		private readonly ILogger<TaxcomDocflowSendEventConsumer> _logger;
		private readonly IEdoDocflowHandler _edoDocflowHandler;

		public TaxcomDocflowSendEventConsumer(
			ILogger<TaxcomDocflowSendEventConsumer> logger,
			IEdoDocflowHandler edoDocflowHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoDocflowHandler = edoDocflowHandler ?? throw new ArgumentNullException(nameof(edoDocflowHandler));
		}

		public async Task Consume(ConsumeContext<TaxcomDocflowSendEvent> context)
		{
			var message = context.Message;

			try
			{
				_logger.LogInformation(
					"Создаем исходящий документооборот с документом {EdoDocument}",
					message.UpdInfo.DocumentId);

				await _edoDocflowHandler.CreateTaxcomDocFlowAndSendDocument(message);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при создании исходящего документооборота с документом {EdoDocument}",
					message.UpdInfo.DocumentId);
			}
		}
	}
}
