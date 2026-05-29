using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public class AddDepositProductValidator
	{
		public Result Validate(Nomenclature addingNomenclature, IAddProductSource source)
		{
			if(source.PaymentTypeSource == PaymentTypeSource.Cashless)
			{
				if(addingNomenclature.Category == NomenclatureCategory.deposit
					&& !source.HasDepositItems
					&& source.HasNonPaidDeliveryItems)
				{
					return Result.Failure(
						new Error(
							"CantAddDepositProductError",
							"Нельзя добавить залоговую позицию, если в заказе уже есть незалоговые позиции."));
				}

				if(addingNomenclature.Category != NomenclatureCategory.deposit
					&& source.HasDepositItems
					&& source.HasNonPaidDeliveryItems)
				{
					return Result.Failure(
						new Error(
							"CantAddNonDepositProductError",
							"Нельзя добавить незалоговую позицию, если в заказе уже есть залоговые позиции."));
				}
			}

			return Result.Success();
		}
	}
}
