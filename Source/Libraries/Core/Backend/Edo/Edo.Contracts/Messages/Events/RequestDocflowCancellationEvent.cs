namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие создания предложения об аннулировании 
	/// документооборота по задаче
	/// </summary>
	public class RequestDocflowCancellationEvent
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
