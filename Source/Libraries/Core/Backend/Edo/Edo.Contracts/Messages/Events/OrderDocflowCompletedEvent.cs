namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие о том, что документооборот по заказу успешно завершён
	/// </summary>
	public class OrderDocflowCompletedEvent
	{
		/// <summary>
		/// Идентификатор документа ЭДО
		/// </summary>
		public int DocumentId { get; set; }
	}
}
