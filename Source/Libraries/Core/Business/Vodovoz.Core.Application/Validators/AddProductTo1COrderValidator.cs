using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Errors.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public class AddProductTo1COrderValidator
	{
		public Result Validate(
			Nomenclature addingNomenclature,
			IAddSaleItemSource source)
		{
			if(source.IsLoadedFrom1C)
			{
				return Result.Failure(OrderErrors.CantAddProductTo1COrder);
			}

			return Result.Success();
		}
	}
}
