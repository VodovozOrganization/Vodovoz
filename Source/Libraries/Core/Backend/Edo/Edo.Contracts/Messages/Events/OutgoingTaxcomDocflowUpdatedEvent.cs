using System;

namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие обновления статуса документооборота в Такском
	/// </summary>
	public class OutgoingTaxcomDocflowUpdatedEvent
	{
		public const string Event = "outgoing-taxcom-docflow-updated";
		/// <summary>
		/// Код кабинета в ЭДО
		/// </summary>
		public string EdoAccount { get; set; }
		/// <summary>
		/// Id главного документа
		/// </summary>
		public string MainDocumentId { get; set; }
		/// <summary>
		/// Id документооборота
		/// </summary>
		public Guid? DocFlowId { get; set; }
		/// <summary>
		/// Статус документооборота
		/// </summary>
		public string Status { get; set; }
		/// <summary>
		/// Статус прослеживаемости в ЧЗ
		/// </summary>
		public string TrueMarkTraceabilityStatus { get; set; }
		/// <summary>
		/// Время изменения состояния
		/// </summary>
		public DateTime StatusChangeDateTime { get; set; }
		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string ErrorDescription { get; set; }
		/// <summary>
		/// Доставлено
		/// </summary>
		public bool IsReceived { get; set; }
	}
}
