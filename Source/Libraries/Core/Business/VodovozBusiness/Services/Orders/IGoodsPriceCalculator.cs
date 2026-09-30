using System.Collections.Generic;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Domain.Service
{
	public interface IGoodsPriceCalculator
	{
		/// <summary>
		/// Расчет стоимости позиции, когда она уже есть в передаваемом списке
		/// </summary>
		/// <param name="saleItemsWithCurrent">Все позиции на продажу(включая текущий, расчетный)</param>
		/// <param name="deliveryPoint">Точка доставки</param>
		/// <param name="counterparty">Клиент</param>
		/// <param name="currentSaleItem">Позиция, для которой идет расчет</param>
		/// <param name="hasPermissionsForAlternativePrice">Есть ли права на установки альтернативной цены</param>
		/// <returns>Стоимость позиции(заказа, онлайн заказа и т.д)</returns>
		(SaleItemPriceType PriceType, decimal Price) CalculateItemPrice(
			IEnumerable<ISaleItem> saleItemsWithCurrent,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			ISaleItem currentSaleItem,
			bool hasPermissionsForAlternativePrice);
		/// <summary>
		/// Расчет стоимости позиции, когда она еще не добавлена в общий список
		/// </summary>
		/// <param name="saleItemsWithoutNew">Позиции на продажу(кроме текущего, расчетного)</param>
		/// <param name="deliveryPoint">Точка доставки</param>
		/// <param name="counterparty">Клиент</param>
		/// <param name="newSaleItem">Новая позиция на продажу</param>
		/// <param name="hasPermissionsForAlternativePrice">Есть ли права на установки альтернативной цены</param>
		/// <returns>Стоимость позиции</returns>
		(SaleItemPriceType PriceType, decimal Price) CalculateItemPrice(
			IEnumerable<ISaleItem> saleItemsWithoutNew,
			DeliveryPoint deliveryPoint,
			Counterparty counterparty,
			IGetFixedPrice newSaleItem,
			bool hasPermissionsForAlternativePrice);
		/// <summary>
		/// Получение цены по общему количеству товара одной номенклатуры, без фиксы
		/// </summary>
		/// <param name="allSaleItems">Список позиций</param>
		/// <param name="saleItem">Рассчитываемый товар</param>
		/// <param name="hasPermissionsForAlternativePrice">Есть права на утсановку альтернативной цены</param>
		/// <param name="doNotCalculateWaterFromPromoSets">Не считать воду из промонаборов</param>
		/// <param name="doNotCalculatePresentsDiscount">Не считать подарки</param>
		/// <returns></returns>
		(SaleItemPriceType PriceType, decimal Price) GetPriceByTotalCount(
			IEnumerable<ISaleItem> allSaleItems,
			INomenclatureCount saleItem,
			bool hasPermissionsForAlternativePrice,
			bool doNotCalculateWaterFromPromoSets = true,
			bool doNotCalculatePresentsDiscount = true);
	}
}
