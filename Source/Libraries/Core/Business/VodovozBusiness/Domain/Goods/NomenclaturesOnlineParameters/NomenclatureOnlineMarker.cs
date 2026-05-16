using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	/// <summary>
	/// Акция в ИПЗ
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum NomenclatureOnlineMarker
	{
		/// <summary>
		/// Товар недели
		/// </summary>
		[Display(Name = "Товар недели")]
		ProductOfWeek,
		/// <summary>
		/// Скидка
		/// </summary>
		[Display(Name = "Скидка")]
		Sale
	}
}
