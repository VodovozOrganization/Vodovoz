using TaxcomEdo.Contracts.Documents;

namespace Edo.Contracts.Messages.Events
{
	public class InformalDocumentFileDataSendEvent
	{
		/// <summary>
		/// Идентификатор документа
		/// </summary>
		public int DocumentId { get; set; }
		public OrderDocumentFileData FileData { get; set; }
	}
}
