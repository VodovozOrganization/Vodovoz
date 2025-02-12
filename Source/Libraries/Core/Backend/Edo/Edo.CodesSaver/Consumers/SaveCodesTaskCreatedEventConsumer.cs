using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.CodesSaver.Consumers
{
	public class SaveCodesTaskCreatedEventConsumer : IConsumer<SaveCodesTaskCreatedEvent>
	{
		private readonly ILogger<SaveCodesTaskCreatedEventConsumer> _logger;
		private readonly SaveCodesEventHandler _saveCodesEventHandler;

		public SaveCodesTaskCreatedEventConsumer(
			ILogger<SaveCodesTaskCreatedEventConsumer> logger,
			SaveCodesEventHandler saveCodesEventHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_saveCodesEventHandler = saveCodesEventHandler ?? throw new ArgumentNullException(nameof(saveCodesEventHandler));
		}

		public async Task Consume(ConsumeContext<SaveCodesTaskCreatedEvent> context)
		{
			try
			{
				await _saveCodesEventHandler.Handle(context.Message.EdoTaskId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке события сохранения кодов");
				//await context.ConsumeCompleted;
			}
		}
	}
}
