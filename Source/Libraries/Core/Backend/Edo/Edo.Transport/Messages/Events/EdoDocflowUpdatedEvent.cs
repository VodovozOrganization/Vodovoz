using System;
using Vodovoz.Core.Domain.Documents;

namespace Edo.Transport.Messages.Events
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
		public EdoDocFlowStatus DocFlowStatus { get; set; }

		/// <summary>
		/// Время обновления статуса документооборота
		/// </summary>
		public DateTime? StatusChangeTime { get; set; }
	}
}
