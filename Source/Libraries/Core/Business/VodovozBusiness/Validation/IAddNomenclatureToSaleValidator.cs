using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Validation
{
	public interface IAddNomenclatureToSaleValidator
	{
		Result CanAddNomenclature(ISaleSource source);
		
		Result CanAddNomenclature(
			Nomenclature nomenclature,
			ISaleSource source
		);
	}
}
