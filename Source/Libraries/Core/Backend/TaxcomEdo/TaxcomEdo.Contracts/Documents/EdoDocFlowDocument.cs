using System;

namespace TaxcomEdo.Contracts.Documents
{
	public class EdoDocFlowDocument
	{
		/// <summary>
		/// Id документа в ERP
		/// </summary>
		public string ExternalIdentifier { get; set; }
		/// <summary>
		/// Внутренний Id документа в хранилище служб Такском
		/// </summary>
		public Guid? InternalId { get; set; }
		/// <summary>
		/// Название документа
		/// </summary>
		public string TransactionCode { get; set; }
	}
}
