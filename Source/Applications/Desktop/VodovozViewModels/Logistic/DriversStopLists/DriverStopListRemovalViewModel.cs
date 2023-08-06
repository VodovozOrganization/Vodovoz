using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace Vodovoz.ViewModels.Logistic.DriversStopLists
{
	public class DriverStopListRemovalViewModel : UowDialogViewModelBase
	{
		public DriverStopListRemovalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			int driverId
			) : base(unitOfWorkFactory, navigation)
		{

		}
	}
}
