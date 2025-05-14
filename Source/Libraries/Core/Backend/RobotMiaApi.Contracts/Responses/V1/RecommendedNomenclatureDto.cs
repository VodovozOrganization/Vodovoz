using System.Text.Json.Serialization;

namespace RobotMiaApi.Contracts.Responses.V1
{
	/// <summary>
	/// Рекомендованный товар
	/// </summary>
	public class RecommendedNomenclatureDto
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		[JsonPropertyName("id")]
		public int Id { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Текущая цена номенклатуры
		/// </summary>
		[JsonPropertyName("actual_price")]
		public float ActualPrice { get; set; }
	}
}
