using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Controllers
{
	public interface IDiscountController
	{
		/// <summary>
		/// Проверка применимости скидки к номенклатуре, т.е. если выбранное основание скидки содержит номенклатуру,
		/// которая указана в основании скидки, либо основание содержит категорию номенклатуры, либо основание содержит товарную группу
		/// с такой номенклатурой, то возвращаем true
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="nomenclature">Номенклатура</param>
		/// <returns>true/false</returns>
		bool IsApplicableDiscount(DiscountReason reason, Nomenclature nomenclature);
	}
}
