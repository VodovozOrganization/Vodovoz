using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Controllers
{
	public interface IDiscountController
	{
		bool IsApplicableDiscount(DiscountReason reason, Nomenclature nomenclature);
	}
}
