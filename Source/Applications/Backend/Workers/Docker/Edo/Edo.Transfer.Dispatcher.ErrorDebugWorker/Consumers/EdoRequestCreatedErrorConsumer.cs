using Edo.Contracts.Messages.Events;
using Edo.Scheduler.Service;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class EdoRequestCreatedErrorConsumer : IConsumer<EdoRequestCreatedEvent>
	{
		private readonly ILogger<EdoRequestCreatedErrorConsumer> _logger;
		private readonly EdoTaskScheduler _taskScheduler;

		public EdoRequestCreatedErrorConsumer(ILogger<EdoRequestCreatedErrorConsumer> logger, EdoTaskScheduler taskScheduler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
		}

		public async Task Consume(ConsumeContext<EdoRequestCreatedEvent> context)
		{
			await _taskScheduler.CreateTask(context.Message.Id, context.CancellationToken);
		}
	}
}

