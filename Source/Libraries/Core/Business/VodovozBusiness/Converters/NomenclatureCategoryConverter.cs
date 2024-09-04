using TaxcomEdo.Contracts.Goods;
using Vodovoz.Core.Domain.Goods;

namespace VodovozBusiness.Converters
{
	public class NomenclatureCategoryConverter : INomenclatureCategoryConverter
	{
		public NomenclatureInfoCategory ConvertNomenclatureCategoryToNomenclatureInfoCategory(NomenclatureCategory nomenclatureCategory)
		{
			if(nomenclatureCategory == NomenclatureCategory.master)
			{
				return NomenclatureInfoCategory.Master;
			}

			if(nomenclatureCategory == NomenclatureCategory.service)
			{
				return NomenclatureInfoCategory.Service;
			}

			return NomenclatureInfoCategory.Other;
		}
	}
}
