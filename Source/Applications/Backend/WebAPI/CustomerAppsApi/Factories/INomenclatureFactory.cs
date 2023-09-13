using CustomerAppsApi.Library.Dto;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Factories
{
	public interface INomenclatureFactory
	{
		NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(
			NomenclatureOnlineParametersData parametersData);
	}
}
