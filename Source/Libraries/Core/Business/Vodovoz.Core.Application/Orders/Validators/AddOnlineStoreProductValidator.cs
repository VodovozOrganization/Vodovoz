using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
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
				return Result.Failure(
					new Error(
						"DontHavePermissionsToAddOnlineStoreProductError",
						"У Вас недостаточно прав для добавления на продажу номенклатуры интернет магазина"));
			}

			return Result.Success();
		}
	}
}
