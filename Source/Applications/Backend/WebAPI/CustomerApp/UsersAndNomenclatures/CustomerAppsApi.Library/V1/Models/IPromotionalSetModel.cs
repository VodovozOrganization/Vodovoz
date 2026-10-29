using CustomerAppsApi.Library.V1.Dto.Goods;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V1.Models
{
	public interface IPromotionalSetModel
	{
		PromotionalSetsDto GetPromotionalSets(Source source);
	}
}
