using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Errors.Sale;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public class AddServiceProductToNonServiceSourceValidator
	{
		public Result Validate(Nomenclature addingNomenclature, IAddSaleItemSource source)
		{
			if(source.Products.Any(x => !NomenclatureEntity.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
				&& addingNomenclature.Category == NomenclatureCategory.master)
			{
				return Result.Failure(SaleErrors.CantAddServiceNomenclatureToNonServiceOrderError());
			}

			return Result.Success();
		}
	}
}
