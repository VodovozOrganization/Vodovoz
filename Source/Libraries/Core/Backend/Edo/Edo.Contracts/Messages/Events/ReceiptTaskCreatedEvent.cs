namespace Edo.Contracts.Messages.Events
{
	public class ReceiptTaskCreatedEvent
	{
		public int ReceiptEdoTaskId { get; set; }
	}

	public class ReceiptSendEvent
	{
		public int ReceiptEdoTaskId { get; set; }
	}

	public class ReceiptSentEvent
	{
		public int ReceiptEdoTaskId { get; set; }
	}

	public class ReceiptCompleteEvent
	{
		public int ReceiptEdoTaskId { get; set; }
	}
}
