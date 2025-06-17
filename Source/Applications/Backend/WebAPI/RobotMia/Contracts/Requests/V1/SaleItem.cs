using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Requests.V1
{
	/// <summary>
	/// Заказываемый товар
	/// </summary>
	public class SaleItem
	{
		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		[JsonPropertyName("nomenclature_id"), Required]
		public int NomenclatureId { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		[JsonPropertyName("count"), Required]
		public decimal Count { get; set; }
	}
}
