
using TransactionalOutbox.Contracts;

namespace Edo.Contracts.Messages.Events
{
	public class EdoRequestCreatedEvent : IIdempotentOutboxMessage
	{
		public int Id { get; set; }

		public int GetAggregateId()
		{
			return Id;
		}

		public string GetDeduplicationKey()
		{
			return $"{nameof(EdoRequestCreatedEvent)}_{Id}";
		}
	}
}
