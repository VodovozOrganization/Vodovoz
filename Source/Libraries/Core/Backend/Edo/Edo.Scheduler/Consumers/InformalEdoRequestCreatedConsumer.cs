using Edo.Contracts.Messages.Events;
using Edo.Scheduler.Service;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Scheduler.Consumers
{
	/// <summary>
	/// Консьюмер для обработки созданной задачи ЭДО неформальной заявки
	/// </summary>
	public class InformalEdoRequestCreatedConsumer : IConsumer<InformalEdoRequestCreatedEvent>
	{
		private readonly ILogger<InformalEdoRequestCreatedConsumer> _logger;
		private readonly EdoTaskScheduler _taskScheduler;

		public InformalEdoRequestCreatedConsumer(ILogger<InformalEdoRequestCreatedConsumer> logger, EdoTaskScheduler taskScheduler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
		}

		public async Task Consume(ConsumeContext<InformalEdoRequestCreatedEvent> context)
		{
			var message = context.Message;
			try
			{
				_logger.LogInformation(
					"Обрабатываем событие {EventName}, InformalRequestId: {RequestId}",
					nameof(InformalEdoRequestCreatedEvent),
					message.InformalRequestId);

				await _taskScheduler.CreateOrderDocumentTask(context.Message.InformalRequestId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при обработке события {EventName}, InformalRequestId: {RequestId}",
					nameof(InformalEdoRequestCreatedEvent),
					message.InformalRequestId);
				throw;
			}
		}
	}
}
