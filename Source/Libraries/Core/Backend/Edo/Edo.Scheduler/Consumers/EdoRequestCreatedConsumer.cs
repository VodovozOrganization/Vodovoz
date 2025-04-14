using Edo.Scheduler.Service;
using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Docflow.Consumers
{
	public class EdoRequestCreatedConsumer : IConsumer<EdoRequestCreatedEvent>
	{
		private readonly ILogger<EdoRequestCreatedConsumer> _logger;
		private readonly EdoTaskScheduler _taskScheduler;

		public EdoRequestCreatedConsumer(ILogger<EdoRequestCreatedConsumer> logger, EdoTaskScheduler taskScheduler)
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
