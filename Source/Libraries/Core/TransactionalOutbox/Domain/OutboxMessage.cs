using System;
using TransactionalOutbox.Contracts;
using TransactionalOutbox.Extensions;

namespace TransactionalOutbox.Domain
{
	public class OutboxMessage
	{
		public virtual Guid Guid { get; protected set; }

		public virtual DateTime CreatedAt { get; protected set; }

		public virtual string Payload { get; protected set; }

		public virtual DateTime? SentAt { get; protected set; }

		public virtual int Attempts { get; protected set; }

		public virtual string Error { get; protected set; }

		public virtual string DeduplicationKey { get; protected set; }

		public virtual string MessageType { get; protected set; }

		protected OutboxMessage() { }
		
		public OutboxMessage(IOutboxDomainEvent notificationDomainEvent, object integrationEvent)
		{
			if(notificationDomainEvent == null)
			{
				throw new ArgumentNullException(nameof(notificationDomainEvent));
			}

			MessageType = integrationEvent.GetType().FullName; 
			Payload = integrationEvent.Serialize();
			DeduplicationKey = notificationDomainEvent.GetDeduplicationKey();
		}

		public virtual void MarkAsSent()
		{
			SentAt = DateTime.UtcNow;
		}

		public virtual void IncrementAttempts(string error = null)
		{
			Attempts++;

			if(!string.IsNullOrEmpty(error))
			{
				Error = error;
			}
		}
	}
}
