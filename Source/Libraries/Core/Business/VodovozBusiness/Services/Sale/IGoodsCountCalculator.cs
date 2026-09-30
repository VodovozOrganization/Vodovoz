using System.Collections.Generic;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Services.Sale
{
	public interface IGoodsCountCalculator
	{
		/// <summary>
		/// Общее количество позиции
		/// </summary>
		/// <param name="saleItem">Позиция на продажу</param>
		/// <param name="saleItems">Все позиции на продажу</param>
		/// <returns></returns>
		decimal TotalItemCount(INomenclatureCount saleItem, IEnumerable<ISaleItem> saleItems);
		/// <summary>
		/// Расчет общего количества 19л воды
		/// </summary>
		/// <param name="saleItems">Товары</param>
		/// <param name="doNotCalculateWaterFromPromoSets">Не брать в расчет промонаборы</param>
		/// <param name="doNotCalculatePresentsDiscount">Не брать в расчет подарки</param>
		/// <returns>Количество 19л воды</returns>
		decimal GetTotalWater19LCount(
			IEnumerable<ISaleItem> saleItems,
			bool doNotCalculateWaterFromPromoSets = false,
			bool doNotCalculatePresentsDiscount = false);
	}
}
