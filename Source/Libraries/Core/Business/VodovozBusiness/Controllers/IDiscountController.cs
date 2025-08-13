using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Controllers
{
	public interface IDiscountController
	{
		/// <summary>
		/// Проверка применимости скидки к номенклатуре
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="nomenclature">Номенклатура</param>
		/// <returns>true/false</returns>
		bool IsApplicableDiscount(DiscountReason reason, Nomenclature nomenclature);
	}
}
