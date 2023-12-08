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

		public void NotifyGlobalBreakAvailability(GlobalBreakAvailability breakAvailability)
		{
			_messageBus.Publish(breakAvailability);
		}

		public void NotifyOperatorsOnBreak(OperatorsOnBreakEvent operatorsOnBreak)
		{
			_messageBus.Publish(operatorsOnBreak);
		}
	}
}
