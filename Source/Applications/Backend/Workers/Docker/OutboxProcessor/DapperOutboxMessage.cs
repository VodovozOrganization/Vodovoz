using System;

namespace OutboxProcessor
{
	public class DapperOutboxMessage
	{
		public int Id { get; set; }

		public DateTime CreatedAt { get; set; }

		public string Payload { get; set; }

		public string CorrelationId { get; set; }

		public DateTime? SentAt { get; set; }

		public int Attempts { get; set; }

		public string Error { get; set; }

		public string DeduplicationKey { get; set; }

		public int? AggregateId { get; set; }

		public string MessageType { get; set; }
	}
}
