using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Rules
{
	public interface IAddPromoSetRule
	{
		Result<string> Apply(IUnitOfWork uow, ISaleSource source, PromotionalSet proSet);
	}
}
