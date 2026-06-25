using System.Collections.Generic;
using CustomerAppsApi.Library.V2.Dto.Goods;
using Vodovoz.Converters;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.V2.Factories
{
	public interface INomenclatureFactory
	{
		NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(NomenclatureOnlineParametersData parametersData);
		NomenclaturesDto CreateNomenclaturesDto(
			INomenclatureOnlineCharacteristicsConverter nomenclatureOnlineCharacteristicsConverter,
			IEnumerable<OnlineNomenclatureNode> onlineNomenclatures);
	}
}
