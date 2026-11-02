namespace Edo.Contracts.Messages.Events
{
	public class TransferDocumentProblemEvent
	{
		public int DocumentId { get; set; }

		public TransferDocumentProblemEvent(int documentId)
		{
			DocumentId = documentId;
		}
	}
}
