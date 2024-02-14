using MassTransit;
using Pacs.Core.Messages.Events;
using Pacs.Server.Breaks;
using System;
using System.Threading.Tasks;

namespace Pacs.Server.Consumers
{
	public class PacsSettingsConsumer : IConsumer<SettingsEvent>
	{
		private readonly GlobalBreakController _globalBreakController;

		public PacsSettingsConsumer(GlobalBreakController globalBreakController)
		{
			_globalBreakController = globalBreakController ?? throw new ArgumentNullException(nameof(globalBreakController));
		}

		public async Task Consume(ConsumeContext<SettingsEvent> context)
		{
			_globalBreakController.UpdateSettings(context.Message.Settings);
			await Task.CompletedTask;
		}
	}
}
