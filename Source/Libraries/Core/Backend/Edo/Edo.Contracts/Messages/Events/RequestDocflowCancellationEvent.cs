namespace Edo.Contracts.Messages.Events
{
	public class RequestDocflowCancellationEvent
	{
		public int TaskId { get; set; }
		public string Reason { get; set; }
	}
}
