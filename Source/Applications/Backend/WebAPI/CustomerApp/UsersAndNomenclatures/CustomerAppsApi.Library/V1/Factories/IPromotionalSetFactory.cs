using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.V1.Dto.Goods;

namespace CustomerAppsApi.Library.V1.Factories
{
	public interface IPromotionalSetFactory
	{
		PromotionalSetsDto CreatePromotionalSetsDto(
			IDictionary<int, PromotionalSetOnlineParametersDto> promoSetParameters,
			ILookup<int, PromotionalSetItemBalanceDto> promoSetItemsData
			);
	}
}
