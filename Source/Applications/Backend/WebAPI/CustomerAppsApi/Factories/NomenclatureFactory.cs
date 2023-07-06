using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Goods.NomenclatureOnlineParameters;

namespace CustomerAppsApi.Factories
{
	public class NomenclatureFactory : INomenclatureFactory
	{
		public NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(
			IDictionary<int, NomenclatureOnlineParameters> parameters)
		{
			return new NomenclaturesPricesAndStockDto
			{
				PricesAndStocks = parameters.Select(keyPairValue => CreateNomenclaturePricesAndStockDto(keyPairValue.Value)).ToList()
			};
		}

		private NomenclaturePricesAndStockDto CreateNomenclaturePricesAndStockDto(NomenclatureOnlineParameters parameters)
		{
			return new NomenclaturePricesAndStockDto
			{
				NomenclatureErpId = parameters.Nomenclature.Id,
				AvailableForSale = parameters.NomenclatureOnlineAvailability,
				Marker = parameters.NomenclatureOnlineMarker,
				PercentDiscount = parameters.NomenclatureOnlineDiscount,
				Prices = CreateNomenclaturePricesDto(parameters.NomenclatureOnlinePrices)
			};
		}

		private IList<NomenclaturePricesDto> CreateNomenclaturePricesDto(IEnumerable<NomenclatureOnlinePrice> onlinePrices)
		{
			return onlinePrices.Select(CreateNomenclaturePricesDto).ToList();
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
