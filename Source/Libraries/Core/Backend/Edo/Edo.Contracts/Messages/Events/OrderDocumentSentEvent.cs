namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие отправки документа
	/// </summary>
	public class OrderDocumentSentEvent
	{
		/// <summary>
		/// Идентификатор документа
		/// </summary>
		public int DocumentId { get; set; }

		public OrderDocumentSentEvent(int documentId)
		{
			DocumentId = documentId;
		}
	}
}
