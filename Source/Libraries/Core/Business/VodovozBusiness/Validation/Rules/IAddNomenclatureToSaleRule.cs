using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Validation.Rules
{
	public interface IAddNomenclatureToSaleRule
	{
		Result Apply(Nomenclature nomenclature, ISaleSource source);
	}
}
