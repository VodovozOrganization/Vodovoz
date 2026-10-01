using Vodovoz.Core.Domain.Results;
using Vodovoz.Errors.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Validation.Rules;

namespace Vodovoz.Core.Application.Validators.Rules
{
	public class AddSaleItemTo1COrderRule : IAddSaleItemRule
	{
		public Result Apply(ISaleSource source)
		{
			if(source.IsLoadedFrom1C)
			{
				return Result.Failure(OrderErrors.CantAddProductTo1COrder);
			}

			return Result.Success();
		}
	}
}
