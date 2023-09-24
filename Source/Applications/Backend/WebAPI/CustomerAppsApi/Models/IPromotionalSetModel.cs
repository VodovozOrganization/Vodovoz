using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Models
{
	public interface IPromotionalSetModel
	{
		PromotionalSetsDto GetPromotionalSets(Source source);
	}
}
