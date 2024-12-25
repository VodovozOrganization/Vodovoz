using Edo.Scheduler.Service;
using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Docflow.Consumers
{
	public class EdoRequestCreatedConsumer : IConsumer<EdoRequestCreatedEvent>
	{
		private readonly EdoTaskScheduler _taskScheduler;

		public EdoRequestCreatedConsumer(EdoTaskScheduler taskScheduler)
		{
			_taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
		}

		public async Task Consume(ConsumeContext<EdoRequestCreatedEvent> context)
		{
			await _taskScheduler.CreateTask(context.Message.Id, context.CancellationToken);
		}
	}
}
