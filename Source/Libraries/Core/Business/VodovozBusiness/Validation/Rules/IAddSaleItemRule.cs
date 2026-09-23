using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Validation.Rules
{
	public interface IAddSaleItemRule
	{
		Result Apply(ISaleSource source);
	}
}
