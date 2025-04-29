using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	/// <summary>
	/// Отмена вывода из оборота
	/// </summary>
	public class DocumentCancellationDto
	{
		/// <summary>
		/// ИНН
		/// </summary>
		[JsonPropertyName("inn")]
		public string Inn { get; set; }

		/// <summary>
		/// Guid документа
		/// </summary>
		[JsonPropertyName("lk_gtin_receipt_id")]
		public string LkGtinReceiptId { get; set; }

		/// <summary>
		/// Версия
		/// </summary>
		[JsonPropertyName("version")]
		public int Version { get; set; }
	}
}
