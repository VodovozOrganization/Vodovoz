using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Errors.Sale;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Validation.Rules;

namespace Vodovoz.Core.Application.Validators.Rules
{
	public class AddNomenclatureToDeliverySaleWithoutDeliveryPointRule : IAddSaleItemRule
	{
		public Result Apply(ISaleSource source)
		{
			if(source.DeliveryPoint is null && !source.IsSelfDelivery)
			{
				Result.Failure(SaleErrors.CantAddSaleItemToDeliverySaleWithoutDeliveryPointError());
			}

			return Result.Success();
		}
	}
}
