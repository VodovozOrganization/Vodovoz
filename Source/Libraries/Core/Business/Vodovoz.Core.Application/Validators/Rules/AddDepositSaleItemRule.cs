using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Errors.Sale;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Validation.Rules;

namespace Vodovoz.Core.Application.Validators.Rules
{
	public class AddDepositSaleItemRule : IAddNomenclatureToSaleRule
	{
		public Result Apply(
			Nomenclature addingNomenclature,
			ISaleSource source
			)
		{
			if(source.PaymentType is PaymentType.Cashless)
			{
				var sourceName = source.GetSubjectNames()?.Nominative ?? "Источник продажи";
				
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
