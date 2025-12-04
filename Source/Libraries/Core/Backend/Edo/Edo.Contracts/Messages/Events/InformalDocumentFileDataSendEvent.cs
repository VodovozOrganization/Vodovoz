using TaxcomEdo.Contracts.Documents;

namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие отправки файла неформализованного документа
	/// </summary>
	public class InformalDocumentFileDataSendEvent
	{
		/// <summary>
		/// Идентификатор документа
		/// </summary>
		public int DocumentId { get; set; }

		/// <summary>
		/// Файл неформализованного документа
		/// </summary>
		public OrderDocumentFileData FileData { get; set; }
	}
}
