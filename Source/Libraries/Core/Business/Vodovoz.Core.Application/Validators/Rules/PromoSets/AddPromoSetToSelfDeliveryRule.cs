using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Rules;

namespace Vodovoz.Core.Application.Validators.Rules.PromoSets
{
	public class AddPromoSetToSelfDeliveryRule : IAddPromoSetRule
	{
		public Result<string> Apply(IUnitOfWork uow, ISaleSource source, PromotionalSet proSet)
		{
			if(source.IsSelfDelivery)
			{
				return Result.Success(string.Empty);
			}

			return null;
		}
	}
}
