using MassTransit;
using Pacs.Core.Messages.Events;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public class OperatorNotifier : IOperatorNotifier
	{
		private readonly IBus _messageBus;

		public OperatorNotifier(IBus messageBus)
		{
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task OperatorChanged(OperatorState operatorState, OperatorBreakAvailability breakAvailability)
		{
			var result = new OperatorStateEvent
			{
				EventId = Guid.NewGuid(),
				State = operatorState,
				BreakAvailability = breakAvailability
			};

			await _messageBus.Publish(result);
		}
	}
}
