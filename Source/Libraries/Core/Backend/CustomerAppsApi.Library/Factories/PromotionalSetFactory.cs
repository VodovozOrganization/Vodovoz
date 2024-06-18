using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Goods;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Factories
{
	public class PromotionalSetFactory : IPromotionalSetFactory
	{
		public PromotionalSetsDto CreatePromotionalSetsDto(PromotionalSetOnlineParametersData parametersData)
		{
			return new PromotionalSetsDto
			{
				PromotionalSets = CreatePromotionalSetDto(parametersData)
			};
		}

		private IList<PromotionalSetDto> CreatePromotionalSetDto(PromotionalSetOnlineParametersData parametersData)
		{
			return parametersData.PromotionalSetOnlineParametersNodes.Select(parametersNode => new PromotionalSetDto
				{
					ErpId = parametersNode.Value.PromotionalSetId,
					AvailableForSale = parametersNode.Value.AvailableForSale,
					OnlineName = parametersNode.Value.PromotionalSetOnlineName,
					ForNewClients = parametersNode.Value.PromotionalSetForNewClients,
					BottlesCountForCalculatingDeliveryPrice = parametersNode.Value.BottlesCountForCalculatingDeliveryPrice,
					PromotionalNomenclatures = CreatePromotionalNomenclatureDto(
						parametersNode.Value.PromotionalSetId, parametersData.PromotionalSetItemBalanceNodes)
				})
				.ToList();
		}

		private IList<PromotionalNomenclatureDto> CreatePromotionalNomenclatureDto(
			int promoSetId, ILookup<int, PromotionalSetItemBalanceNode> promoSetItems)
		{
			var items = promoSetItems[promoSetId];
			return !items.Any()
				? new List<PromotionalNomenclatureDto>()
				: items.Select(CreatePromotionalNomenclatureDto).ToList();
		}

		private PromotionalNomenclatureDto CreatePromotionalNomenclatureDto(PromotionalSetItemBalanceNode promoSetItem)
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
