using System.Collections.Generic;
using Vodovoz.Core.Data.Sale;
using Vodovoz.Domain.Orders;

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
		/// Создание дто строки на продажу с детализацией скидок из пакета аренды
		/// </summary>
		/// <param name="freeRentPackage">Пакет аренды</param>
		/// <returns></returns>
		OnlineOrderItemWithDiscountDetailsDto CreateWithDiscountDetailsDto(OnlineFreeRentPackage freeRentPackage);
	}
}
