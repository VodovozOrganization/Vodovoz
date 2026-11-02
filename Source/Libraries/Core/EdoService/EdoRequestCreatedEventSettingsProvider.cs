using Edo.Contracts.Messages.Events;
using TransactionalOutbox.Abstractions;

namespace EdoService.Library
{
	public class EdoRequestCreatedEventSettingsProvider :
		IOutboxSettingsProvider<EdoRequestCreatedEvent>
	{
		public bool IsDisabled(EdoRequestCreatedEvent notificationEvent)
		{
			return false;
		}

		public bool IsDuplicateAllowed(EdoRequestCreatedEvent notificationEvent)
		{
			return false;
		}
	}
}
