using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Responses.V1
{
	/// <summary>
	/// Товар на продажу
	/// </summary>
	public class OrderSaleItemDto
	{
		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		[JsonPropertyName("nomenclature_id")]
		public int NomenclatureId { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Текущая цена номенклатуры (не цена из заказа)
		/// </summary>
		[JsonPropertyName("actual_price")]
		public decimal ActualPrice { get; set; }

		/// <summary>
		/// Идентификатор промонабора (если указан)
		/// </summary>
		[JsonPropertyName("promo_set_id")]
		public int? PromoSetId { get; set; }

		/// <summary>
		/// Количество (из заказа)
		/// </summary>
		[JsonPropertyName("count")]
		public decimal Count { get; set; }
	}
}
