using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Goods;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Factories
{
	public interface IPromotionalSetFactory
	{
		PromotionalSetsDto CreatePromotionalSetsDto(PromotionalSetOnlineParametersData parametersData);
	}
}
