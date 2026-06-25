using System.Collections.Generic;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Список цен номенклатур
	/// </summary>
	public class NomenclaturesPricesAndStockDto
	{
		/// <summary>
		/// Список
		/// </summary>
		public IList<NomenclaturePricesAndStockDto> PricesAndStocks { get; set; }
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}
