namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие создания задачи по Тендеру
	/// </summary>
	public class TenderTaskCreatedEvent
	{
		/// <summary>
		/// Id задачи по Тендеру
		/// </summary>
		public int TenderEdoTaskId { get; set; }
	}
}
