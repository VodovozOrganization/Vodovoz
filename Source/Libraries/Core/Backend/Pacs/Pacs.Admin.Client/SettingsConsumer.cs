using MassTransit;
using Pacs.Core.Messages.Events;
using System;
using System.Threading.Tasks;

namespace Pacs.Admin.Client
{
	public class SettingsConsumer : IConsumer<SettingsEvent>
	{
		public Task Consume(ConsumeContext<SettingsEvent> context)
		{
			throw new NotImplementedException();
		}
	}
}
