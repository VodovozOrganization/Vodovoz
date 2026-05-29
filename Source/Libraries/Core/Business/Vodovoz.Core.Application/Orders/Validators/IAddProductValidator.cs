using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public interface IAddProductValidator
	{
		Result Validate(Nomenclature addingNomenclature, IAddProductSource source);
	}
}
