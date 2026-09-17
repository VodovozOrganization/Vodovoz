using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Errors.Sale;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public class AddNonServiceProductToServiceSourceValidator
	{
		public Result Validate(Nomenclature addingNomenclature, IAddSaleItemSource source)
		{
			if(source.Products.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
				&& !NomenclatureEntity.GetCategoriesForMaster().Contains(addingNomenclature.Category))
			{
				return Result.Failure(SaleErrors.CantAddNonServiceNomenclatureToServiceOrderError());
			}

			return Result.Success();
		}
	}
}
