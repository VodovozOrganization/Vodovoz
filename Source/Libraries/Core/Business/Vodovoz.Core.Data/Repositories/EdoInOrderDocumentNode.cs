using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.Repositories
{
	public class EdoInOrderDocumentNode
	{
		public DateTime RequestTime { get; set; }
		public int RequestId { get; set; }
		public EdoRequestSource RequestSource { get; set; }
		public string ManualRequestAuthor { get; set; }
		public int TaskId { get; set; }
		public EdoTaskStatus TaskStatus { get; set; }
		public EdoTaskType TaskType { get; set; }

		/// <summary>
		/// Количество кодов, которые привязаны к заявке ЭДО.
		/// </summary>
		public int? CodesInRequest { get; set; }

		/// <summary>
		/// Количество кодов, которые использованы в задаче.
		/// Это коды которые находяться в строках задачи.
		/// </summary>
		public int? CodesUsedInTask { get; set; }

		public OrderDocumentType? InformalOrderDocumentType { get; set; }
		public DocumentEdoTaskStage? TaskUpdStage { get; set; }
		public EdoReceiptStatus? TaskReceiptStage { get; set; }
		public TenderEdoTaskStage? TaskTenderStage { get; set; }

		/// <summary>
		/// Статус документа в ЭДО
		/// </summary>
		public EdoDocumentStatus? EdoDocumentStatus { get; set; }

		/// <summary>
		/// Причина отмены
		/// </summary>
		public string CancellationReason { get; set; }
	}
}
