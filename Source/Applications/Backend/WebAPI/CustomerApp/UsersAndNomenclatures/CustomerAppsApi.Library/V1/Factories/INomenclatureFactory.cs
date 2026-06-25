using System.Collections.Generic;
using CustomerAppsApi.Library.V1.Dto.Goods;
using Vodovoz.Converters;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.V1.Factories
{
	public interface INomenclatureFactory
	{
		NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(NomenclatureOnlineParametersData parametersData);
		NomenclaturesDto CreateNomenclaturesDto(
			INomenclatureOnlineCharacteristicsConverter nomenclatureOnlineCharacteristicsConverter,
			IEnumerable<OnlineNomenclatureNode> onlineNomenclatures);
	}
}
