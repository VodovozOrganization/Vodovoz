using System;

namespace Pacs.Core.Idenpotency
{
	public interface IIdempotencyEvent
	{
		Guid EventId { get; }
	}
}
