using System.Linq;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Errors.Sale;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Validation.Rules;

namespace Vodovoz.Core.Application.Validators.Rules
{
	public class AddNonServiceItemToServiceSourceRule : IAddNomenclatureToSaleRule
	{
		public Result Apply(
			Nomenclature addingNomenclature,
			ISaleSource source
			)
		{
			var sourceName = source.GetSubjectNames()?.Nominative ?? "Источник продажи";
			
			if(source.SaleItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
				&& !NomenclatureEntity.GetCategoriesForMaster().Contains(addingNomenclature.Category))
			{
				return Result.Failure(SaleErrors.CantAddNonServiceNomenclatureToServiceOrderError(sourceName));
			}

			return Result.Success();
		}
	}
}
