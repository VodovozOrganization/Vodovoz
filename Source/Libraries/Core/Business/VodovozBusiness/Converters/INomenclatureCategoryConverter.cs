using TaxcomEdo.Contracts.Goods;
using Vodovoz.Core.Domain.Goods;

namespace VodovozBusiness.Converters
{
	public interface INomenclatureCategoryConverter
	{
		NomenclatureInfoCategory ConvertNomenclatureCategoryToNomenclatureInfoCategory(NomenclatureCategory nomenclatureCategory);
	}
}
