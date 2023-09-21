using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.Dto
{
	public class NomenclaturePricesAndStockDto
	{
		public int NomenclatureErpId { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public GoodsOnlineAvailability? AvailableForSale { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public NomenclatureOnlineMarker? Marker { get; set; }
		public decimal? PercentDiscount { get; set; }
		public IList<NomenclaturePricesDto> Prices { get; set; }
	}
}
