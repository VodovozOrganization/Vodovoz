namespace TaxcomEdo.Contracts.DocflowDocuments
{
	public class DocumentWithMessage
	{
		/// <summary>
		/// Тип документа у которому относится сообщение
		/// </summary>
		public DocumentWithMessageType DocumentType { get; set; }

		/// <summary>
		/// Сообщение в документе
		/// </summary>
		public string Message { get; set; }
	}
}
