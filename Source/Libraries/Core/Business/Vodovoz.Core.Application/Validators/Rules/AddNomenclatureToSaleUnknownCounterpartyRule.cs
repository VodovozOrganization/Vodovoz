using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Errors.Sale;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Validation.Rules;

namespace Vodovoz.Core.Application.Validators.Rules
{
	public class AddNomenclatureToSaleUnknownCounterpartyRule : IAddSaleItemRule
	{
		public Result Apply(ISaleSource source)
		{
			if(source.Counterparty is null)
			{
				Result.Failure(SaleErrors.CantAddSaleItemWithoutCounterpartyError());
			}

			return Result.Success();
		}
	}
}
