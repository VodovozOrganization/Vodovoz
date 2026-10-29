namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие для подписания входящего документооборота
	/// </summary>
	public class AcceptingWaitingForCancellationDocflowEvent
	{
		public const string Event = "accepting-waiting-for-cancellation-docflow-event";

		/// <summary>
		/// Код кабинета в ЭДО
		/// </summary>
		public string EdoAccount { get; set; }

		/// <summary>
		/// Id документооборота
		/// </summary>
		public string DocFlowId { get; set; }
	}
}
