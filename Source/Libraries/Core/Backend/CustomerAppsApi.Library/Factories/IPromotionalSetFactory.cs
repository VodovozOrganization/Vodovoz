using CustomerAppsApi.Library.Dto;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Factories
{
	public interface IPromotionalSetFactory
	{
		PromotionalSetsDto CreatePromotionalSetsDto(PromotionalSetOnlineParametersData parametersData);
	}
}
