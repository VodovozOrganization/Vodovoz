using System;

namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие для обновления статуса документооборота в Erp
	/// </summary>
	public class EdoDocflowUpdatedEvent
	{
		public const string Event = "EdoDocflowUpdated";

		/// <summary>
		/// Id ЭДО документа
		/// </summary>
		public int EdoDocumentId { get; set; }

		/// <summary>
		/// Id документооборота
		/// </summary>
		public Guid? DocFlowId { get; set; }

		/// <summary>
		/// Общий статус документооборота
		/// </summary>
		public string DocFlowStatus { get; set; }
		
		/// <summary>
		/// Статус прослеживаемости в ЧЗ
		/// </summary>
		public string TrueMarkTraceabilityStatus { get; set; }

		/// <summary>
		/// Время обновления статуса документооборота
		/// </summary>
		public DateTime? StatusChangeTime { get; set; }
	}
}
