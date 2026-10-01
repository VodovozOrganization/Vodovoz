using VodovozBusiness.Validation;

namespace VodovozBusiness.Factories
{
	public interface IAddPromoSetValidatorFactory
	{
		IAddPromoSetValidator CreateForOrder();
		IAddPromoSetValidator Create();
	}
}
