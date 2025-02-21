namespace Edo.Contracts.Messages.Events
{
	public class ReceiptTaskCreatedEvent
	{
		public int EdoTaskId { get; set; }
	}

	public class ReceiptSendEvent
	{
		public int EdoTaskId { get; set; }
	}

	public class ReceiptSentEvent
	{
		public int EdoTaskId { get; set; }
	}

	public class ReceiptCompleteEvent
	{
		public int EdoTaskId { get; set; }
	}
}
