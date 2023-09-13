using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Nodes
{
	public class NomenclatureOnlineParametersData
	{
		public NomenclatureOnlineParametersData(
			IDictionary<int, NomenclatureOnlineParametersNode> parametersNodes,
			ILookup<int, NomenclatureOnlinePriceNode> onlinePricesNodes)
		{
			NomenclatureOnlineParametersNodes = parametersNodes;
			NomenclatureOnlinePricesNodes = onlinePricesNodes;
		}
		
		public IDictionary<int, NomenclatureOnlineParametersNode> NomenclatureOnlineParametersNodes { get; }
		public ILookup<int, NomenclatureOnlinePriceNode> NomenclatureOnlinePricesNodes { get; }

	}
}