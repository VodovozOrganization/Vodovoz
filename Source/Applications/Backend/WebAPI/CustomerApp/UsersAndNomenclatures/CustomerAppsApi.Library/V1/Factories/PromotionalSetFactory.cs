using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.V1.Dto.Goods;

namespace CustomerAppsApi.Library.V1.Factories
{
	public class PromotionalSetFactory : IPromotionalSetFactory
	{
		public PromotionalSetsDto CreatePromotionalSetsDto(
			IDictionary<int, PromotionalSetOnlineParametersDto> promoSetParameters,
			ILookup<int, PromotionalSetItemBalanceDto> promoSetItemsData)
		{
			return new PromotionalSetsDto
			{
				PromotionalSets = CreatePromotionalSetDto(promoSetParameters, promoSetItemsData)
			};
		}

		private IList<PromotionalSetDto> CreatePromotionalSetDto(
			IDictionary<int, PromotionalSetOnlineParametersDto> promoSetParameters,
			ILookup<int, PromotionalSetItemBalanceDto> promoSetItemsData)
		{
			return promoSetParameters.Select(parametersNode => new PromotionalSetDto
				{
					ErpId = parametersNode.Value.PromotionalSetId,
					AvailableForSale = parametersNode.Value.AvailableForSale,
					OnlineName = parametersNode.Value.PromotionalSetOnlineName,
					ForNewClients = parametersNode.Value.PromotionalSetForNewClients,
					BottlesCountForCalculatingDeliveryPrice = parametersNode.Value.BottlesCountForCalculatingDeliveryPrice,
					PromotionalNomenclatures = CreatePromotionalNomenclatureDto(
						parametersNode.Value.PromotionalSetId, promoSetItemsData)
				})
				.ToList();
		}

		private IList<PromotionalNomenclatureDto> CreatePromotionalNomenclatureDto(
			int promoSetId, ILookup<int, PromotionalSetItemBalanceDto> promoSetItems)
		{
			var items = promoSetItems[promoSetId];
			return !items.Any()
				? new List<PromotionalNomenclatureDto>()
				: items.Select(CreatePromotionalNomenclatureDto).ToList();
		}

		private PromotionalNomenclatureDto CreatePromotionalNomenclatureDto(PromotionalSetItemBalanceDto promoSetItem)
		{
			return new PromotionalNomenclatureDto
			{
				Count = promoSetItem.Count,
				Discount = promoSetItem.Discount,
				ErpNomenclatureId = promoSetItem.NomenclatureId,
				IsDiscountMoney = promoSetItem.IsDiscountMoney
			};
		}
	}
}
