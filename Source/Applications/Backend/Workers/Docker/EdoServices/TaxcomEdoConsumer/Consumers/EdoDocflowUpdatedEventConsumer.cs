using System;
using System.Threading.Tasks;
using Edo.Transport2;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace TaxcomEdoConsumer.Consumers
{
	public class EdoDocflowUpdatedEventConsumer : IConsumer<EdoDocflowUpdatedEvent>
	{
		private readonly ILogger<EdoDocflowUpdatedEventConsumer> _logger;

		public EdoDocflowUpdatedEventConsumer(
			ILogger<EdoDocflowUpdatedEventConsumer> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<EdoDocflowUpdatedEvent> context)
		{
			var message = context.Message;

			try
			{
				
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при уведомлении об обновлении исходящего документооборота с документом {EdoDocument}",
					message.MainDocumentId);
			}
		}
	}
}
