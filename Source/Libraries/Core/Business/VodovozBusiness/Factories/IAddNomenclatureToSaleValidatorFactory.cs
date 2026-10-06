using VodovozBusiness.Validation;

namespace VodovozBusiness.Factories
{
	public interface IAddNomenclatureToSaleValidatorFactory
	{
		/// <summary>
		/// Создание валидатора добавления номенклатур
		/// </summary>
		/// <returns>Валидатор</returns>
		IAddNomenclatureToSaleValidator Create();
	}
}
