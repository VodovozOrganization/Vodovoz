using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Errors.Sale;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public class AddOnlineStoreProductValidator
	{
		public Result Validate(
			Nomenclature addingNomenclature,
			IAddSaleItemSource source,
			bool canAddOnlineStoreNomenclatures = false)
		{
			if(addingNomenclature.OnlineStore != null && !canAddOnlineStoreNomenclatures)
			{
				return Result.Failure(SaleErrors.DontHavePermissionsToAddOnlineStoreProductError());
			}

			return Result.Success();
		}
	}
}
