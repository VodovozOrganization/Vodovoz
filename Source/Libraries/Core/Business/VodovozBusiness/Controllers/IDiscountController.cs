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
			DiscountReason addingDiscount,
			IApplyDiscountReasonItem saleItem
		);
	}
}
