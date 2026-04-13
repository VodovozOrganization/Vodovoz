namespace Edo.Contracts.Messages.Events
{
	public class TransferCompleteEvent
	{
		public int TransferIterationId { get; set; }
		public string TransferInitiator { get; set; }
	}
}
