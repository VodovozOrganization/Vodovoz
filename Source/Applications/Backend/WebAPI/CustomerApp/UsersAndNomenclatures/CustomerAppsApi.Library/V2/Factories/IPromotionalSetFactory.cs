using CustomerAppsApi.Library.V2.Dto.Goods;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.V2.Factories
{
	public interface IPromotionalSetFactory
	{
		PromotionalSetsDto CreatePromotionalSetsDto(PromotionalSetOnlineParametersData parametersData);
	}
}
