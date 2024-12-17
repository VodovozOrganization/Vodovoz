using System;

namespace Pacs.Core.Idenpotency
{
	public interface IPacsEventIdempotencyService<TEvent>
		where TEvent : class, IIdempotencyEvent
	{
		void RegisterProcessed(Guid eventId);
		void RegisterProcessed(IIdempotencyEvent idempotencyEvent);
		bool WasProcessedBefore(Guid eventId);
		bool WasProcessedBefore(IIdempotencyEvent idempotencyEvent);
	}
}
