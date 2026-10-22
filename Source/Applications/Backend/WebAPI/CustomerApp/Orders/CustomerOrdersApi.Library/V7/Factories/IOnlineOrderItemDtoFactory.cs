using System.Collections.Generic;
using Vodovoz.Core.Data.Sale;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Factories
{
	public interface IOnlineOrderItemDtoFactory
	{
		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок
		/// </summary>
		/// <param name="saleItem">Позиция на продажу</param>
		/// <returns></returns>
		OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(IProduct saleItem);
		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок
		/// </summary>
		/// <param name="onlinePromoSet">Онлайн промонабор</param>
		/// <returns></returns>
		OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineOrderPromoSet onlinePromoSet);
		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок из промонабров
		/// </summary>
		/// <param name="promoSetSaleItems">Кортеж из промонабора и позиций на продажу с ним</param>
		/// <returns></returns>
		IEnumerable<OnlineOrderItemWithDiscountDetailsDto> CreateWithDiscountDetailsDto(
			IEnumerable<(PromotionalSet PromoSet, IEnumerable<IProduct> SaleItems)> promoSetSaleItems);
		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок из промонабора
		/// </summary>
		/// <param name="promoSetOrderItems">Кортеж из промонабора и строк заказа с ним</param>
		/// <returns></returns>
		IEnumerable<OnlineOrderItemWithDiscountDetailsDto> CreateWithDiscountDetailsDto(
			IEnumerable<(PromotionalSet PromoSet, IEnumerable<OrderItem> OrderItems)> promoSetOrderItems);
		/// <summary>
		/// Создание дто строки на продажу с детализацией скидок из пакета аренды
		/// </summary>
		/// <param name="freeRentPackage">Пакет аренды</param>
		/// <returns></returns>
		OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineFreeRentPackage freeRentPackage);
	}
}
