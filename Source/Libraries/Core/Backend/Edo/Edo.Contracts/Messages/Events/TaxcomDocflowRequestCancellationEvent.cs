namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие для запроса аннулирования документа по ЭДО
	/// </summary>
	public class TaxcomDocflowRequestCancellationEvent
	{
		public const string Event = "taxcom-docflow-request-cancellation";

		/// <summary>
		/// Код кабинета в ЭДО, от имени которого отправляется запрос
		/// </summary>
		public string EdoAccount { get; set; }

		/// <summary>
		/// Id ЭДО документа
		/// </summary>
		public int DocumentId { get; set; }

		/// <summary>
		/// Идентификатор документооборота старого счета, у которого нет документа нового ЭДО.
		/// </summary>
		public System.Guid? LegacyBillDocflowId { get; set; }

		/// <summary>
		/// Причина аннулирования
		/// </summary>
		public string CancellationReason { get; set; }
	}
}
