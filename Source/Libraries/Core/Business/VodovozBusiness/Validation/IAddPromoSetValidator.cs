using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Validation
{
	public interface IAddPromoSetValidator
	{
		Result<string> CanAddPromotionalSet(IUnitOfWork uow, ISaleSource source, PromotionalSet proSet);
	}
}
