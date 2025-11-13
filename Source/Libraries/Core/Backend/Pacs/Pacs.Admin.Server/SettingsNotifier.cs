using MassTransit;
using Pacs.Core.Messages.Events;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Admin.Server
{
	public class SettingsNotifier : ISettingsNotifier
	{
		private readonly IBus _messageBus;

		public SettingsNotifier(IBus messageBus)
		{
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task SettingsChanged(DomainSettings settings)
		{
			var settingsEvent = new SettingsEvent
			{
				EventId = Guid.NewGuid(),
				Settings = settings
			};

			await _messageBus.Publish(settingsEvent);
		}
	}
}
