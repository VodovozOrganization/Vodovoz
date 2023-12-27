using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Library.Models
{
	public interface INomenclatureModel
	{
		NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks(Source source);
		NomenclaturesDto GetNomenclatures(Source source);
	}
}
