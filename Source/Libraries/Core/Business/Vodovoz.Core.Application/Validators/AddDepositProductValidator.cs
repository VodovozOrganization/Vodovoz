using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Errors.Sale;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public class AddDepositProductValidator
	{
		public Result Validate(Nomenclature addingNomenclature, IAddSaleItemSource source)
		{
			if(source.PaymentTypeSource == PaymentTypeSource.Cashless)
			{
				var sourceName = source.GetDisplayNAme.Nomitive;
				
				if(addingNomenclature.Category == NomenclatureCategory.deposit
					&& !source.HasDeposits
					&& source.HasNonPaidDeliveries)
				{
					return Result.Failure(SaleErrors.CantAddDepositItemError(sourceName));
				}

				if(addingNomenclature.Category != NomenclatureCategory.deposit
					&& source.HasDeposits
					&& source.HasNonPaidDeliveries)
				{
					return Result.Failure(SaleErrors.CantAddNonDepositItemError(sourceName));
				}
			}

			return Result.Success();
		}
	}
}
