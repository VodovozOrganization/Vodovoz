using CustomerOrders.Abstractions.V8.Sale;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Controllers
{
	public interface IDiscountController
	{
		/// <summary>
		/// Проверка применимости скидки к позиции
		/// </summary>
		/// <param name="addingDiscount">Добавляемая скидка</param>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <returns>При успешном выполнении Result.Success, иначе Result.Failure с указанием проблемы</returns>
		Result IsApplicableDiscount(
			DiscountReasonBase addingDiscount,
			IApplicablePromotion saleItem
		);

		/// <summary>
		/// Подсчет Скидки в деньгах из основания скидки
		/// </summary>
		/// <param name="currentRawPrice">Стоимость товара/позиции</param>
		/// <param name="discountReason">Основание скидки</param>
		/// <returns></returns>
		decimal CalculateMoneyDiscount(
			decimal currentRawPrice,
			DiscountReasonBase discountReason
		);
	}
}
