using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Models
{
	public interface INomenclatureModel
	{
		NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks(Source source);
	}
}
