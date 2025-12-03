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
			try
			{
				_logger.LogInformation(
					$"Получено событие {nameof(InformalEdoRequestCreatedEvent)}, DocumentId: {context.Message.InformalRequestId}");

				await _taskScheduler.CreateOrderDocumentTask(context.Message.InformalRequestId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex, $"Обнаружена ошибка при обработке события {nameof(InformalEdoRequestCreatedEvent)}, DocumentId: {context.Message.InformalRequestId}");
				throw;
			}
		}
	}
}
