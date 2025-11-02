namespace Edo.Contracts.Messages.Events
{
	public class ReceiptTaskCreatedEvent
	{
		public int ReceiptEdoTaskId { get; set; }
	}

	public class ReceiptReadyToSendEvent
	{
		public int ReceiptEdoTaskId { get; set; }
	}

	public class ReceiptCompleteEvent
	{
		public int ReceiptEdoTaskId { get; set; }
	}
}
