﻿using System;

namespace Edo.Transport2
{
	/// <summary>
	/// Событие для обновления статуса документооборота в Erp
	/// </summary>
	public class EdoDocflowUpdatedEvent
	{
		public const string Event = "EdoDocflowUpdated";
		/// <summary>
		/// Id главного документа
		/// </summary>
		public string MainDocumentId { get; set; }
		/// <summary>
		/// Id документооборота
		/// </summary>
		public Guid? DocFlowId { get; set; }
		/// <summary>
		/// Общий статус документооборота
		/// </summary>
		public string DocFlowStatus { get; set; }
		/// <summary>
		/// Время завершения документооборота
		/// </summary>
		public DateTime? AcceptTime { get; set; }
	}
}
