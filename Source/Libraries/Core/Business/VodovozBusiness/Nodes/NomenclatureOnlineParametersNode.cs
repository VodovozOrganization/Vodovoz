using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Nodes
{
	public class NomenclatureOnlineParametersNode
	{
		public int Id { get; set; }
		public int NomenclatureId { get; set; }
		public GoodsOnlineAvailability? AvailableForSale { get; set; }
		public NomenclatureOnlineMarker? Marker { get; set; }
		public decimal? PercentDiscount { get; set; }
	}
}
