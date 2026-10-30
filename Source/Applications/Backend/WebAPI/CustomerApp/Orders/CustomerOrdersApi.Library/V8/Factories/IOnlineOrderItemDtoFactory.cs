using CustomerOrders.Contracts.V8.Orders.Items;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Factories
{
	public interface IOnlineOrderItemDtoFactory
	{
		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок
		/// </summary>
		/// <param name="saleItem">Позиция на продажу</param>
		/// <param name="parameterType">Тип онлайн параметра по источнику заказа <see cref="GoodsOnlineParameterType"/></param>
		/// <returns></returns>
		OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(IProduct saleItem, GoodsOnlineParameterType parameterType);

		/// <summary>
		/// Создание дто строк на продажу с детализацией скидок
		/// </summary>
		/// <param name="allSaleItems">Все продаваемые позиции</param>
		/// <param name="deliveryPointFixedPrices">Фиксированные цены из точки доставки</param>
		/// <param name="counterpartyPointFixedPrices">Фиксированные цены клиента</param>
		/// <param name="parameterType">Тип онлайн параметра по источнику заказа <see cref="GoodsOnlineParameterType"/></param>
		/// <returns></returns>
		IEnumerable<OnlineOrderItemWithDiscountDetailsDto> CreateWithDiscountDetailsDto(
			IEnumerable<ISaleItem> allSaleItems,
			IEnumerable<NomenclatureFixedPrice> deliveryPointFixedPrices,
			IEnumerable<NomenclatureFixedPrice> counterpartyPointFixedPrices,
			GoodsOnlineParameterType parameterType
		);

		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок из промонабров
		/// </summary>
		/// <param name="promoSets">Промонаборы</param>
		/// <returns></returns>
		IEnumerable<OnlineOrderItemWithDiscountDetailsDto> CreateWithDiscountDetailsDto(IEnumerable<PromotionalSet> promoSets);

		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок из промонабора
		/// </summary>
		/// <param name="promoSet">Промонабор</param>
		/// <param name="count">Количество</param>
		/// <returns></returns>
		OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(PromotionalSet promoSet, decimal count);

		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок из промонабора
		/// </summary>
		/// <param name="onlinePromoSet">Промонабор из онлайн заказа</param>
		/// <returns></returns>
		OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineOrderPromoSet onlinePromoSet);

		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок из пакета аренды
		/// </summary>
		/// <param name="freeRentPackage">Пакет аренды</param>
		/// <returns></returns>
		OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineFreeRentPackage freeRentPackage);

		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок из промонабора
		/// </summary>
		/// <param name="onlinePromoSet">Промонабор из онлайн заказа</param>
		/// <returns></returns>
		OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineOrderTemplatePromoSet onlinePromoSet);

		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок из пакета аренды
		/// </summary>
		/// <param name="templateRentPackage">Пакет аренды</param>
		/// <returns></returns>
		OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineOrderTemplateFreeRentPackage templateRentPackage);
	}
}
