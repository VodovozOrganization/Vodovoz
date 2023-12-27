using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Library.Models
{
	public interface IPromotionalSetModel
	{
		PromotionalSetsDto GetPromotionalSets(Source source);
	}
}
