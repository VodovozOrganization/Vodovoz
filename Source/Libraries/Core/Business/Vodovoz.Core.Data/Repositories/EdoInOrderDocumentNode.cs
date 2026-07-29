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
		public int TaskId { get; set; }
		public EdoTaskStatus TaskStatus { get; set; }
		public EdoTaskType TaskType { get; set; }
		public int? CodesQuantity { get; set; }
		public OrderDocumentType? InformalOrderDocumentType { get; set; }
		public DocumentEdoTaskStage? TaskUpdStage { get; set; }
		public EdoReceiptStatus? TaskReceiptStage { get; set; }
		public TenderEdoTaskStage? TaskTenderStage { get; set; }

		/// <summary>
		/// Статус документа в ЭДО
		/// </summary>
		public EdoDocumentStatus? EdoDocumentStatus { get; set; }

		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string ErrorDescription { get; set; }
	}
}
