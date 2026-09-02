using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces.Sale;
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
		/// Расчет детализации скидки в деньгах по основаниям скидки, включая персональную скидку
		/// Подходит для случаев, когда надо раскрыть информацию по скидкам из заказа или онлайн заказа
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <returns>Скидка в деньгах</returns>
		(decimal TotalDiscount, IDictionary<int, IDiscountAmount> DiscountDetails) CalculateTotalDiscountDetails(
			ICalculatingTotalMoneyDiscount saleItem
		);
	}
}
