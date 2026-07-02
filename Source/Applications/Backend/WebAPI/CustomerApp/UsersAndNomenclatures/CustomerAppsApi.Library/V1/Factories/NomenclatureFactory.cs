using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.V1.Dto.Goods;

namespace CustomerAppsApi.Library.V1.Factories
{
	public class NomenclatureFactory : INomenclatureFactory
	{
		public NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(
			IDictionary<int, NomenclatureOnlineParametersDto> nomenclatureParameters,
			ILookup<int, NomenclatureOnlinePriceDto> prices)
		{
			return new NomenclaturesPricesAndStockDto
			{
				PricesAndStocks = CreateNomenclaturePricesAndStockDto(nomenclatureParameters, prices)
			};
		}

		public NomenclaturesDto CreateNomenclaturesDto(IEnumerable<OnlineNomenclatureDto> nomenclaturesData)
		{
			return new NomenclaturesDto
			{
				OnlineNomenclatures = nomenclaturesData
			};
		}

		private IList<NomenclaturePricesAndStockDto> CreateNomenclaturePricesAndStockDto(
			IDictionary<int, NomenclatureOnlineParametersDto> nomenclatureParameters,
			ILookup<int, NomenclatureOnlinePriceDto> prices)
		{
			return nomenclatureParameters.Select(parametersNode => new NomenclaturePricesAndStockDto
				{
					NomenclatureErpId = parametersNode.Value.NomenclatureId,
					AvailableForSale = parametersNode.Value.AvailableForSale,
					Marker = parametersNode.Value.Marker,
					PercentDiscount = parametersNode.Value.PercentDiscount,
					Prices = CreateNomenclaturePricesDto(parametersNode.Value.Id, prices)
				})
				.ToList();
		}
		
		private IList<NomenclaturePricesDto> CreateNomenclaturePricesDto(
			int parametersId, ILookup<int, NomenclatureOnlinePriceDto> onlinePrices)
		{
			var prices = onlinePrices[parametersId];
			return !prices.Any()
				? new List<NomenclaturePricesDto>()
				: prices.Select(CreateNomenclaturePricesDto).ToList();
		}

		private NomenclaturePricesDto CreateNomenclaturePricesDto(NomenclatureOnlinePriceDto onlinePrice)
		{
			return new NomenclaturePricesDto
			{
				MinCount = onlinePrice.MinCount,
				Price = onlinePrice.Price,
				PriceWithoutDiscount = onlinePrice.PriceWithoutDiscount
			};
		}
	}
}
