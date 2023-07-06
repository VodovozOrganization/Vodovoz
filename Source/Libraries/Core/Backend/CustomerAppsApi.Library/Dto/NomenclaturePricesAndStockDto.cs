using System.Collections.Generic;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.Dto
{
	public class NomenclaturePricesAndStockDto
	{
		public int NomenclatureErpId { get; set; }
		public NomenclatureOnlineAvailability? AvailableForSale { get; set; }
		public NomenclatureOnlineMarker? Marker { get; set; }
		public decimal? PercentDiscount { get; set; }
		public IList<NomenclaturePricesDto> Prices { get; set; }
	}
}
