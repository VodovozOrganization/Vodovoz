using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.Dto
{
	public class PromotionalSetDto
	{
		public int ErpId { get; set; }
		public string OnlineName { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public GoodsOnlineAvailability? AvailableForSale { get; set; }
		public bool ForNewClients { get; set; }
		public IList<PromotionalNomenclatureDto> PromotionalNomenclatures { get; set; }
	}
}
