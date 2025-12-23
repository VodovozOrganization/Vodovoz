namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие аннулирования неформального документа
	/// </summary>
	public class InformalOrderDocumentCancelledEvent
	{
		public int DocumentId { get; set; }
	}
}
