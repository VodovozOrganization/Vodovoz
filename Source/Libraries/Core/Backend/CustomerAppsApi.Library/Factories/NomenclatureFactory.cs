using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto.Goods;
using Vodovoz.Converters;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Factories
{
	public class NomenclatureFactory : INomenclatureFactory
	{
		public NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(NomenclatureOnlineParametersData parametersData)
		{
			return new NomenclaturesPricesAndStockDto
			{
				PricesAndStocks = CreateNomenclaturePricesAndStockDto(parametersData)
			};
		}
		
		public NomenclaturesDto CreateNomenclaturesDto(
			INomenclatureOnlineCharacteristicsConverter nomenclatureOnlineCharacteristicsConverter,
			IEnumerable<OnlineNomenclatureNode> onlineNomenclatures)
		{
			return new NomenclaturesDto
			{
				OnlineNomenclatures = new List<OnlineNomenclatureDto>(
					onlineNomenclatures.Select(x => OnlineNomenclatureDto.Create(nomenclatureOnlineCharacteristicsConverter, x)))
			};
		}
		
		private IList<NomenclaturePricesAndStockDto> CreateNomenclaturePricesAndStockDto(NomenclatureOnlineParametersData parametersData)
		{
			return parametersData.NomenclatureOnlineParametersNodes.Select(parametersNode => new NomenclaturePricesAndStockDto
				{
					NomenclatureErpId = parametersNode.Value.NomenclatureId,
					AvailableForSale = parametersNode.Value.AvailableForSale,
					Marker = parametersNode.Value.Marker,
					PercentDiscount = parametersNode.Value.PercentDiscount,
					Prices = CreateNomenclaturePricesDto(parametersNode.Value.Id, parametersData.NomenclatureOnlinePricesNodes)
				})
				.ToList();
		}
		
		private IList<NomenclaturePricesDto> CreateNomenclaturePricesDto(
			int parametersId, ILookup<int, NomenclatureOnlinePriceNode> onlinePrices)
		{
			var prices = onlinePrices[parametersId];
			return !prices.Any()
				? new List<NomenclaturePricesDto>()
				: prices.Select(CreateNomenclaturePricesDto).ToList();
		}

		private NomenclaturePricesDto CreateNomenclaturePricesDto(NomenclatureOnlinePriceNode onlinePrice)
		{
			return new NomenclaturePricesDto
			{
				MinCount = onlinePrice.MinCount,
				Price = onlinePrice.Price,
				PriceWithoutDiscount = onlinePrice.PriceWithoutDiscount
			};
		}

		private NomenclaturePricesDto CreateNomenclaturePricesDto(NomenclatureOnlinePrice onlinePrice)
		{
			return new NomenclaturePricesDto
			{
				MinCount = onlinePrice.NomenclaturePrice.MinCount,
				Price = onlinePrice.NomenclaturePrice.Price,
				PriceWithoutDiscount = onlinePrice.PriceWithoutDiscount
			};
		}
	}
}
