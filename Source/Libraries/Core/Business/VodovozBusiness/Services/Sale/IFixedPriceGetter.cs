using System.Collections.Generic;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Services.Sale
{
	public interface IFixedPriceGetter
	{
		/// <summary>
		/// Получение номенклатур с фиксой
		/// </summary>
		/// <param name="saleSource">Источник продажи(заказ, шаблон)</param>
		/// <returns></returns>
		IEnumerable<Nomenclature> GetNomenclaturesWithFixedPrices(ISaleSource saleSource);

		/// <summary>
		/// Получение фиксы
		/// </summary>
		/// <param name="deliveryPoint">Точка доставки</param>
		/// <param name="counterparty">Клиент</param>
		/// <param name="allSaleItems">Все продаваемые позиции</param>
		/// <param name="saleItem">Позиция на продажу по которой идет проверка</param>
		/// <returns></returns>
		(SaleItemPriceType PriceType, decimal Price)? GetFixedPriceOrNull(
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IEnumerable<ISaleItem> allSaleItems,
			ISaleItem saleItem
		);

		/// <summary>
		/// Получение фиксы
		/// </summary>
		/// <param name="deliveryPoint">Точка доставки</param>
		/// <param name="counterparty">Клиент</param>
		/// <param name="saleItem">Позиция на продажу</param>
		/// <param name="bottlesCount">Общее количество бутылей</param>
		/// <returns></returns>
		(SaleItemPriceType PriceType, decimal Price)? GetFixedPriceOrNull(
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IGetFixedPrice saleItem,
			decimal bottlesCount
		);
	}
}
