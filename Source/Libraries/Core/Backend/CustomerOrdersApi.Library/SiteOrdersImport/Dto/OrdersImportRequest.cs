using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Dto
{
	/// <summary>
	/// Пакет выгрузки заказов и брошенных корзин, который сайт присылает на наш приёмный endpoint.
	/// Сайт сам инициирует POST в ночном окне.
	/// </summary>
	public class OrdersImportRequest
	{
		/// <summary>
		/// Токен авторизации запроса.
		/// </summary>
		[JsonPropertyName("token")]
		public string Token { get; set; }

		/// <summary>
		/// Версия схемы данных (например, "v1").
		/// </summary>
		[JsonPropertyName("contract_version")]
		public string ContractVersion { get; set; }

		/// <summary>
		/// Время формирования пакета на стороне сайта.
		/// </summary>
		[JsonPropertyName("sent_at")]
		public string SentAt { get; set; }

		/// <summary>
		/// Уникальный идентификатор пакета (для логов и идемпотентности).
		/// </summary>
		[JsonPropertyName("batch_id")]
		public string BatchId { get; set; }

		/// <summary>
		/// Размер пакета — максимум записей в <see cref="Items"/>.
		/// </summary>
		[JsonPropertyName("limit")]
		public int Limit { get; set; }

		/// <summary>
		/// Сколько записей на стороне сайта всего ожидает передачи на момент формирования пакета (справочно).
		/// </summary>
		[JsonPropertyName("total_count")]
		public int TotalCount { get; set; }

		/// <summary>
		/// Массив сущностей пакета: заказы и брошенные корзины.
		/// </summary>
		[JsonPropertyName("items")]
		public IReadOnlyList<OrderImportItem> Items { get; set; }
	}
}
