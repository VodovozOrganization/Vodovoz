using System.Collections.Generic;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Factories
{
	public interface INomenclatureFactory
	{
		NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(
			IDictionary<int, NomenclatureOnlineParameters> parameters);
	}
}
