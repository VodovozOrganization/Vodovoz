using System.Collections.Generic;

namespace CustomerAppsApi.Library.Dto
{
	public class NomenclaturesPricesAndStockDto
	{
		public IList<NomenclaturePricesAndStockDto> PricesAndStocks { get; set; }
		public string ErrorMessage { get; set; }
	}
}
