using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.ViewModels.Factories
{
	public interface ICarVersionsViewModelFactory
	{
		CarVersionsViewModel CreateCarVersionsViewModel(Car car);
	}
}
