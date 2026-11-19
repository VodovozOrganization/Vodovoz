using System.Collections.Generic;

namespace CustomerAppsApi.Library.V3.Dto.Goods
{
	/// <summary>
	/// Список цен номенклатур
	/// </summary>
	public class SaleItemsPricesAndStockDto
	{
		/// <summary>
		/// Список
		/// </summary>
		public IEnumerable<SaleItemPricesDto> PricesAndStocks { get; set; }
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}
