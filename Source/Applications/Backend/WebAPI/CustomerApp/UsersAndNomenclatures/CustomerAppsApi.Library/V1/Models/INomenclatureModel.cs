using CustomerAppsApi.Library.V1.Dto.Goods;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V1.Models
{
	public interface INomenclatureModel
	{
		NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks(Source source);
		NomenclaturesDto GetNomenclatures(Source source);
	}
}
