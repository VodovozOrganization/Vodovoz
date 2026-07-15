using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V2.Dto.Goods.Attributes
{
	/// <summary>
	/// Характеристики воды и лимонадов
	/// </summary>
	public class WaterSaleItemAttributes
	{
		/// <summary>
		/// Объем тары
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TareVolume? TareVolume { get; set; }
		/// <summary>
		/// Одноразовая тара
		/// </summary>
		public bool IsDisposableTare { get; set; }
		/// <summary>
		/// Новая бутыль
		/// </summary>
		public bool IsNewBottle { get; set; }
		/// <summary>
		/// Газированная вода
		/// </summary>
		public bool IsSparklingWater { get; set; }
	}
}
