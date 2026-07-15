using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Dto
{
	/// <summary>
	/// Ответ нашей стороны на принятый пакет выгрузки.
	/// Возвращается в теле HTTP-ответа на запрос приёма пакета.
	/// </summary>
	public class OrdersImportResponse
	{
		/// <summary>
		/// Общий признак успешной обработки пакета.
		/// </summary>
		[JsonPropertyName("success")]
		public bool Success { get; set; }

		/// <summary>
		/// Идентификатор пакета — должен совпадать с <c>batch_id</c> из запроса.
		/// </summary>
		[JsonPropertyName("batch_id")]
		public string BatchId { get; set; }

		/// <summary>
		/// Идентификаторы записей, которые успешно приняты и сохранены.
		/// </summary>
		[JsonPropertyName("imported_order_ids")]
		public IReadOnlyList<long> ImportedOrderIds { get; set; } = new List<long>();

		/// <summary>
		/// Идентификаторы записей, которые не удалось сохранить без ручной обработки.
		/// </summary>
		[JsonPropertyName("error_order_ids")]
		public IReadOnlyList<long> ErrorOrderIds { get; set; } = new List<long>();
	}
}
