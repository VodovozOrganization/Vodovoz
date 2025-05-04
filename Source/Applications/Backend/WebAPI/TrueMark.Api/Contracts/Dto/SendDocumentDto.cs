using System.Text.Json.Serialization;

namespace TrueMark.Api.Contracts.Dto
{
	/// <summary>
	/// Отправка документа в ЧЗ
	/// </summary>
	public class SendDocumentDto
	{
		/// <summary>
		/// Формат
		/// </summary>
		[JsonPropertyName("document_format")]
		public string DocumentFormat { get; set; }

		/// <summary>
		/// Документ в base64
		/// </summary>
		[JsonPropertyName("product_document")]
		public string ProductDocument { get; set; }

		/// <summary>
		/// Тип
		/// </summary>
		[JsonPropertyName("type")]
		public string Type { get; set; }

		/// <summary>
		/// Подпись
		/// </summary>
		[JsonPropertyName("signature")]
		public string Signature { get; set; }
	}
}
