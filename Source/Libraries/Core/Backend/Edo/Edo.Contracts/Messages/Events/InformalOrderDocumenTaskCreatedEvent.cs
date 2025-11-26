namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие создания задачи по неформальному документу заказа
	/// </summary>
	public class InformalOrderDocumenTaskCreatedEvent
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int InformalOrderDocumentTaskId { get; set; }
	}
}
