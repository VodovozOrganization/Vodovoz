using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pacs.Core.Idenpotency
{
	internal class PacsEventIdempotencyService<TEvent>
		: IPacsEventIdempotencyService<TEvent>
		where TEvent : class, IIdempotencyEvent
	{
		private const int _maxEventsPoolSize = 100;
		private ConcurrentQueue<Guid> _processedEventsPool;

		public PacsEventIdempotencyService()
		{
			_processedEventsPool = new ConcurrentQueue<Guid>();
		}

		public bool WasProcessedBefore(Guid eventId) => _processedEventsPool.Any(x => x == eventId);
		public bool WasProcessedBefore(IIdempotencyEvent idempotencyEvent) => _processedEventsPool.Any(x => x == idempotencyEvent.EventId);

		public void RegisterProcessed(Guid eventId)
		{
			while(_processedEventsPool.Count >= _maxEventsPoolSize)
			{
				_processedEventsPool.TryDequeue(out _);
			}

			_processedEventsPool.Enqueue(eventId);
		}

		public void RegisterProcessed(IIdempotencyEvent idempotencyEvent)
		{
			while(_processedEventsPool.Count >= _maxEventsPoolSize)
			{
				_processedEventsPool.TryDequeue(out _);
			}

			_processedEventsPool.Enqueue(idempotencyEvent.EventId);
		}
	}
}
