using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Goods;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Models
{
	public interface IPromotionalSetModel
	{
		PromotionalSetsDto GetPromotionalSets(Source source);
	}
}
