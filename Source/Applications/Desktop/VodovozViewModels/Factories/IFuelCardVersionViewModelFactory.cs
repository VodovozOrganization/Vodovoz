using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.ViewModels.Factories
{
	public interface IFuelCardVersionViewModelFactory
	{
		FuelCardVersionViewModel CreateFuelCardVersionViewModel(Car car, IUnitOfWork unitOfWork);
	}
}
