namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие создания заявки на отмену ЭДО задачи
	/// </summary>
	public class RequestTaskCancellationEvent
	{
		/// <summary>
		/// Id отменяемой ЭДО задачи
		/// </summary>
		public int TaskId { get; set; }

		/// <summary>
		/// Причина отмены
		/// </summary>
		public string Reason { get; set; }
	}
}
