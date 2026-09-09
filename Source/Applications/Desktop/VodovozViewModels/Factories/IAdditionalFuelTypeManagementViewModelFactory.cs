using QS.DomainModel.UoW;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.ViewModels.Factories
{
	public interface IAdditionalFuelTypeManagementViewModelFactory
	{
		AdditionalFuelTypeManagementViewModel CreateAdditionalFuelTypeManagementViewModel(Car car, IUnitOfWork uow, DialogViewModelBase parentDialog);
	}
}
