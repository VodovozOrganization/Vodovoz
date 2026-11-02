using Edo.Contracts.Messages.Events;
using TransactionalOutbox.Abstractions;

namespace EdoService.Library
{
	public class AcceptingWaitingForCancellationDocflowEventSettingsProvider :
		IOutboxSettingsProvider<AcceptingWaitingForCancellationDocflowEvent>
	{
		public bool IsDisabled(AcceptingWaitingForCancellationDocflowEvent notificationEvent)
		{
			return false;
		}

		public bool IsDuplicateAllowed(AcceptingWaitingForCancellationDocflowEvent notificationEvent)
		{
			return false;
		}
	}
}
