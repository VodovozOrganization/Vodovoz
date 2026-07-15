using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Goods;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.V2.Repositories
{
	/// <summary>
	/// Контракт репозитория продаваемых позиций
	/// </summary>
	public interface ISaleItemRepository
	{
		/// <summary>
		/// Получение всех позиций для выгрузки в ИПЗ(номенклатуры, промонаборы, пакеты аренды) <see cref="AggregatedSaleItems"/>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<AggregatedSaleItems> GetAggregatedSaleItemsAsync(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType,
			CancellationToken cancellationToken
			);

		/// <summary>
		/// Получение данных по ценам и остаткам
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<AggregatedSaleItemPrices> GetAggregatedSaleItemPricesAsync(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType,
			CancellationToken cancellationToken
			);

		/// <summary>
		/// Получение параметров выгружаемых номенклатур
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <returns></returns>
		Task<IEnumerable<NomenclatureOnlineParametersDto>> GetNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType
		);

		/// <summary>
		/// Получение цен выгружаемых номенклатур
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <returns></returns>
		Task<IEnumerable<NomenclatureOnlinePriceDto>> GetNomenclaturesOnlinePricesForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType
		);

		/// <summary>
		/// Получение параметров выгружаемых промонаборов
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <returns></returns>
		Task<IEnumerable<SaleItemPricesDto>> GetPromoSetOnlineParametersForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType
		);

		/// <summary>
		/// Получение цен и остатков товаров выгружаемых промонаборов
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <param name="warehouseIds"></param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<IEnumerable<PromotionalSetItemBalanceDto>> GetPromotionalSetsItemsWithBalanceForSendAsync(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType,
			IEnumerable<int> warehouseIds,
			CancellationToken cancellationToken
		);

		/// <summary>
		/// Получение данных по ценам выгружаемых пакетов аренды
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType"></param>
		/// <returns></returns>
		Task<IEnumerable<SaleItemPricesDto>> GetFreeRentPackagePricesForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType
		);

		/// <summary>
		/// Получение данных остаткам
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="parameterType">Получатель параметров</param>
		/// <param name="warehouseIds">Идентификаторы складов, по которым будет выборка остатков</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<IEnumerable<(int NomenclatureId, decimal Stock)>> GetNomenclaturesForSendInStockAsync(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType,
			IEnumerable<int> warehouseIds,
			CancellationToken cancellationToken
		);
	}
}
