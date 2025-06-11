using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Responses.V1
{
	/// <summary>
	/// Строка рекомендации
	/// </summary>
	public class RecomendationItemDto
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		[JsonPropertyName("id")]
		public int Id { get; set; }

		/// <summary>
		/// Идентификатор рекомендации
		/// </summary>
		[JsonPropertyName("recomendation_id")]
		public int RecomendationId { get; set; }

		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		[JsonPropertyName("nomenclature_id")]
		public int NomenclatureId { get; set; }
	}
}
