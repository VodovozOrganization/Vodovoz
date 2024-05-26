using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Widgets.Cars.Insurance;

namespace Vodovoz.ViewModels.Factories
{
	public interface ICarInsuranceVersionViewModelFactory
	{
		CarInsuranceVersionViewModel CreateKaskoCarInsuranceVersionViewModel(Car car);
		CarInsuranceVersionViewModel CreateOsagoCarInsuranceVersionViewModel(Car car);
	}
}