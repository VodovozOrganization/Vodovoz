using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
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
				return Result.Failure(new Error("CantAddProductTo1COrder", "Нельзя добавлять товары в заказ с 1С"));
			}

			return Result.Success();
		}
	}
}
