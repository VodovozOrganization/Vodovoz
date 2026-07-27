using System.Collections.Generic;
using CustomerAppsApi.Library.V2.Dto.Goods;

namespace CustomerAppsApi.Library.V2.Factories
{
	/// <summary>
	/// Интерфейс фабрики продаваемых товаров/услуг
	/// </summary>
	public interface ISaleItemFactory
	{
		/// <summary>
		/// Создание данных с ценами продаваемых товаров/услуг
		/// </summary>
		/// <param name="nomenclatureParameters">Параметры товаров</param>
		/// <param name="nomenclaturePrices">Цены товаров</param>
		/// <returns></returns>
		IEnumerable<SaleItemPricesDto> CreateSelItemPricesDto(
			IEnumerable<NomenclatureOnlineParametersDto> nomenclatureParameters,
			IEnumerable<NomenclatureOnlinePriceDto> nomenclaturePrices
			);
		/// <summary>
		/// Создание данных о продаваемых товарах/услугах
		/// </summary>
		/// <param name="saleItems">Товары/услуги</param>
		/// <param name="availableWaterIds">Идентификаторы воды для пакетов аренды</param>
		/// <returns></returns>
		SaleItemsDto CreateSaleItemsDto(AggregatedSaleItems saleItems, IEnumerable<int> availableWaterIds);
		/// <summary>
		/// Создание окончательного ответа с информацией по ценам
		/// </summary>
		/// <param name="saleItemPrices">Информация по ценам</param>
		/// <returns></returns>
		SaleItemsPricesAndStockDto CreateSaleItemsPricesAndStockDto(IEnumerable<SaleItemPricesDto> saleItemPrices);
	}
}
