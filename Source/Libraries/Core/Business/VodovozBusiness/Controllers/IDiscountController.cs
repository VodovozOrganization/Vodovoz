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
			IApplicableDiscount saleItem
		);
		
		/// <summary>
		/// Расчет детализации скидки в деньгах по основаниям скидки, исключая персональную скидку
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <returns>Скидка в деньгах</returns>
		(decimal TotalDiscount, IEnumerable<IDiscountAmount> DiscountDetails) CalculateTotalDiscountDetails(
			ICalculatingTotalMoneyDiscount saleItem
		);
	}
}
