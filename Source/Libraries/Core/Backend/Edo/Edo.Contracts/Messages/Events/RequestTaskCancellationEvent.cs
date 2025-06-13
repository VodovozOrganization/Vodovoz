namespace Edo.Contracts.Messages.Events
{
	public class RequestTaskCancellationEvent
	{
		public int TaskId { get; set; }

		public string Reason { get; set; }
	}
}
