using Edo.Scheduler.Service;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Scheduler.Consumers
{
	/// <summary>
	/// Обработчик события создания заявки на вывод из оборота
	/// </summary>
	public class WithdrawalEdoRequestCreatedConsumer : IConsumer<WithdrawalEdoRequestCreatedEvent>
	{
		private readonly ILogger<WithdrawalEdoRequestCreatedConsumer> _logger;
		private readonly EdoTaskScheduler _taskScheduler;

		public WithdrawalEdoRequestCreatedConsumer(
			ILogger<WithdrawalEdoRequestCreatedConsumer> logger, 
			EdoTaskScheduler taskScheduler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
		}

		public async Task Consume(ConsumeContext<WithdrawalEdoRequestCreatedEvent> context)
		{
			_logger.LogInformation("Получено событие создания заявки на вывод из оборота. Id: {Id}", context.Message.Id);
			await _taskScheduler.CreateTask(context.Message.Id, context.CancellationToken);
			_logger.LogInformation("Событие создания заявки на вывод из оборота успешно обработано. Id: {Id}", context.Message.Id);
		}
	}
}
