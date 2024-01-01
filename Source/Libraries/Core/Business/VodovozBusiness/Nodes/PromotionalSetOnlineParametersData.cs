using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Nodes
{
	public class PromotionalSetOnlineParametersData
	{
		public PromotionalSetOnlineParametersData(
			IDictionary<int, PromotionalSetOnlineParametersNode> parametersNodes,
			ILookup<int, PromotionalSetItemBalanceNode> onlinePricesNodes)
		{
			PromotionalSetOnlineParametersNodes = parametersNodes;
			PromotionalSetItemBalanceNodes = onlinePricesNodes;
		}
		
		public IDictionary<int, PromotionalSetOnlineParametersNode> PromotionalSetOnlineParametersNodes { get; }
		public ILookup<int, PromotionalSetItemBalanceNode> PromotionalSetItemBalanceNodes { get; }
	}
}
