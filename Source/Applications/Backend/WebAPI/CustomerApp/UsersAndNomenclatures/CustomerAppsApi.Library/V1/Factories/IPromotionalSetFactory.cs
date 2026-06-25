using CustomerAppsApi.Library.V1.Dto.Goods;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.V1.Factories
{
	public interface IPromotionalSetFactory
	{
		PromotionalSetsDto CreatePromotionalSetsDto(PromotionalSetOnlineParametersData parametersData);
	}
}
