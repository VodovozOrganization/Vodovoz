namespace Edo.Contracts.Messages.Events
{
	public class TransferDocumentAcceptedEvent
	{
		public int DocumentId { get; set; }

		public TransferDocumentAcceptedEvent(int documentId)
		{
			DocumentId = documentId;
		}
	}
}
