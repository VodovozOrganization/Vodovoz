using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Dto
{
	/// <summary>
	/// Одна сущность пакета выгрузки с сайта — заказ или брошенная корзина.
	/// Состав <see cref="Payload"/> определяется контрактом (VVWEB-378 v1) и на этом этапе
	/// сохраняется как есть, без разбора.
	/// </summary>
	public class OrderImportItem
	{
		/// <summary>
		/// Идентификатор записи на стороне сайта (для корзины — идентификатор корзины)
		/// </summary>
		[JsonPropertyName("order_id")]
		public long OrderId { get; set; }

		/// <summary>
		/// Тип сущности: <c>order</c> (заказ) или <c>abandoned_cart</c> (брошенная корзина)
		/// </summary>
		[JsonPropertyName("entity_type")]
		public string EntityType { get; set; }

		/// <summary>
		/// Время последнего изменения записи на стороне сайта (формат "Y-m-d H:i:s")
		/// </summary>
		[JsonPropertyName("updated_at")]
		public string UpdatedAt { get; set; }

		/// <summary>
		/// Статус записи на стороне сайта
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Полезная нагрузка записи по контракту v1. Хранится сырым JSON до согласования разбора.
		/// </summary>
		[JsonPropertyName("payload")]
		public JsonElement Payload { get; set; }
	}
}
