namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие о проблеме с неформализованным документом
	/// </summary>
	public class InformalOrderDocumentProblemEvent
	{
		public int DocumentId { get; set; }
	}
}
