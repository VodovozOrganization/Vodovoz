using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Nodes
{
	public class PromotionalSetOnlineParametersNode
	{
		public int Id { get; set; }
		public int PromotionalSetId { get; set; }
		public GoodsOnlineAvailability? AvailableForSale { get; set; }
		public string PromotionalSetOnlineName { get; set; }
		public bool PromotionalSetForNewClients { get; set; }
		public int? BottlesCountForCalculatingDeliveryPrice { get; set; }
	}
}
