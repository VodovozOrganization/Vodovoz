using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.Dto.Goods
{
	/// <summary>
	/// Цены номенклатуры
	/// </summary>
	public class NomenclaturePricesAndStockDto
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int NomenclatureErpId { get; set; }
		/// <summary>
		/// Доступность для продажи
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public GoodsOnlineAvailability? AvailableForSale { get; set; }
		/// <summary>
		/// Ярлычок навешиваемый на карточку товара в ИПЗ
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public NomenclatureOnlineMarker? Marker { get; set; }
		/// <summary>
		/// Скидка в процентах
		/// </summary>
		public decimal? PercentDiscount { get; set; }
		/// <summary>
		/// Список цен
		/// </summary>
		public IList<NomenclaturePricesDto> Prices { get; set; }
	}
}
