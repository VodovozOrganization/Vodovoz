using System.Collections.Generic;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Goods;
using Vodovoz.Converters;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Factories
{
	public interface INomenclatureFactory
	{
		NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(NomenclatureOnlineParametersData parametersData);
		NomenclaturesDto CreateNomenclaturesDto(
			INomenclatureOnlineCharacteristicsConverter nomenclatureOnlineCharacteristicsConverter,
			IEnumerable<OnlineNomenclatureNode> onlineNomenclatures);
	}
}
