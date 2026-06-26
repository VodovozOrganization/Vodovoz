using CustomerAppsApi.Library.V2.Dto.Goods;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V2.Models
{
	public interface INomenclatureModel
	{
		NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks(Source source);
		SaleItemsDto GetNomenclatures(Source source);
	}
}
