using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Goods;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Models
{
	public interface INomenclatureModel
	{
		NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks(Source source);
		NomenclaturesDto GetNomenclatures(Source source);
	}
}
