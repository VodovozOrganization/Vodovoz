using System;

namespace TransactionalOutbox.Contracts
{
	public interface IOutboxDomainEvent
	{
		string GetDeduplicationKey();
		int GetAggregateId();
	}
}
