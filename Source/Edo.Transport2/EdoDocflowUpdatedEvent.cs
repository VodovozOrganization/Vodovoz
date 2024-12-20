using System;

namespace Edo.Transport2
{
	/// <summary>
	/// Событие для обновления статуса документооборота в Erp
	/// </summary>
	public class EdoDocflowUpdatedEvent
	{
		/// <summary>
		/// Id главного документа
		/// </summary>
		public string MainDocumentId { get; set; }
		/// <summary>
		/// Id документооборота
		/// </summary>
		public Guid DocFlowId { get; set; }
	}
}
