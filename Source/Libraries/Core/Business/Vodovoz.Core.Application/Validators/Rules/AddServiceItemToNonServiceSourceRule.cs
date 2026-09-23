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
	public class AddServiceItemToNonServiceSourceRule : IAddNomenclatureToSaleRule
	{
		public Result Apply(
			Nomenclature addingNomenclature,
			ISaleSource source
			)
		{
			var sourceName = source.GetSubjectNames()?.Nominative ?? "Источник продажи";
			
			if(source.SaleItems.Any(x => !NomenclatureEntity.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
				&& addingNomenclature.Category == NomenclatureCategory.master)
			{
				return Result.Failure(SaleErrors.CantAddServiceNomenclatureToNonServiceOrderError(sourceName));
			}

			return Result.Success();
		}
	}
}
