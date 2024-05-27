using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services.Cars.Insurance;
using Vodovoz.ViewModels.Widgets.Cars.Insurance;

namespace Vodovoz.ViewModels.Factories
{
	public interface ICarInsuranceVersionViewModelFactory
	{
		CarInsuranceVersionViewModel CreateKaskoCarInsuranceVersionViewModel(Car car, ICarInsuranceVersionService carInsuranceVersionService);
		CarInsuranceVersionViewModel CreateOsagoCarInsuranceVersionViewModel(Car car, ICarInsuranceVersionService carInsuranceVersionService);
	}
}
