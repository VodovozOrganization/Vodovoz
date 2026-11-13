using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V3.Dto.Goods;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V3.Services
{
	/// <summary>
	/// Интерфейс сервиса работы с товарами/услугами
	/// </summary>
	public interface ISaleItemService
	{
		/// <summary>
		/// Получение информации по ценам продаваемых товаров/услуг.
		/// Для мобильного приложения дополнительно рассчитывает в Mindbox количество
		/// бонусов, начисляемых за 1 шт. по базовой цене каждой позиции.
		/// Ошибки расчёта бонусов не приводят к ошибке метода: цены и остатки
		/// возвращаются всегда, а непросчитанные позиции получают <c>bonusAmount = null</c>.
		/// </summary>
		/// <param name="source">Источник запроса (ИПЗ). Расчёт бонусов выполняется только для <see cref="Source.MobileApp"/></param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Ответ со списком цен и остатков. У каждой позиции заполнено поле <c>bonusAmount</c>:
		/// <c>0</c> — бонусы за позицию не начисляются, <c>null</c> — расчёт не выполнялся
		/// или Mindbox не ответил.
		/// </returns>
		Task<SaleItemsPricesAndStockDto> GetSaleItemsPricesAndStocksAsync(Source source, CancellationToken cancellationToken);

		/// <summary>
		/// Получение информации по продаваемым товарам/услугам
		/// </summary>
		/// <param name="source">Источник запроса (ИПЗ)</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список продаваемых товаров/услуг</returns>
		Task<SaleItemsDto> GetSaleItemsAsync(Source source, CancellationToken cancellationToken);
	}
}
