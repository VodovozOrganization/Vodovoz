using MassTransit;
using Pacs.Core.Messages.Events;
using System;

namespace Pacs.Server
{
	public class BreakAvailabilityNotifier : IBreakAvailabilityNotifier
	{
		private readonly IBus _messageBus;

		public BreakAvailabilityNotifier(IBus messageBus)
		{
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}
		public void NotifyBreakAvailability(bool breakAvailable)
		{
			var message = new BreakAvailabilityEvent { BreakAvailable = breakAvailable };
			_messageBus.Publish(message);
		}
	}
}
