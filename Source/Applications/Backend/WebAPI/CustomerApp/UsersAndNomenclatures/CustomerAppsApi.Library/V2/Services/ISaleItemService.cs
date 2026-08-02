using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Goods;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V2.Services
{
	/// <summary>
	/// Интерфейс сервиса работы с товарами/услугами
	/// </summary>
	public interface ISaleItemService
	{
		/// <summary>
		/// Получение информации по ценам продаваемых товаров/услуг
		/// </summary>
		/// <param name="source">Источник запроса (ИПЗ)</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<SaleItemsPricesAndStockDto> GetSaleItemsPricesAndStocksAsync(Source source, CancellationToken cancellationToken);

		/// <summary>
		/// Получение информации по продаваемым товарам/услугам
		/// </summary>
		/// <param name="source">Источник запроса (ИПЗ)</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<SaleItemsDto> GetSaleItemsAsync(Source source, CancellationToken cancellationToken);
	}
}
