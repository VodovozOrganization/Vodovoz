using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Models
{
	public interface IPromotionalSetModel
	{
		PromotionalSetsDto GetPromotionalSets(Source source);
	}
}
