using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	public class SaleItemPricesDto
	{
		/// <summary>
		/// Id сущности в ДВ
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Тип товара/услуги
		/// </summary>
		public SaleItemType Type { get; set; }
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
		public IEnumerable<SaleItemPriceDto> Prices { get; set; }
	}
}
