namespace Edo.Contracts.Messages.Events
{
	public class OrderDocumentAcceptedEvent
	{
		public int DocumentId { get; set; }

		public OrderDocumentAcceptedEvent(int documentId)
		{
			DocumentId = documentId;
		}
	}
}
