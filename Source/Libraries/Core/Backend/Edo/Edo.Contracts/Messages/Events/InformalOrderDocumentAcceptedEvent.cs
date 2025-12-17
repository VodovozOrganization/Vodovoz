namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие принятия неформализованного документа
	/// </summary>
	public class InformalOrderDocumentAcceptedEvent
	{
		public int DocumentId { get; set; }
	}
}
