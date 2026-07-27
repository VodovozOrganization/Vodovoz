using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.V1.Dto.Goods;

namespace CustomerAppsApi.Library.V1.Factories
{
	public interface INomenclatureFactory
	{
		NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(
			IDictionary<int, NomenclatureOnlineParametersDto> nomenclatureParameters,
			ILookup<int, NomenclatureOnlinePriceDto> prices
			);
		
		NomenclaturesDto CreateNomenclaturesDto(IEnumerable<OnlineNomenclatureDto> nomenclaturesData);
	}
}
