using CustomerAppsApi.Library.Dto;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Factories
{
	public interface IPromotionalSetFactory
	{
		PromotionalSetsDto CreatePromotionalSetsDto(PromotionalSetOnlineParametersData parametersData);
	}
}
