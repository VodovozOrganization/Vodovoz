using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;

namespace Vodovoz.ViewModels.Logistic.DriversStopLists
{
	public class DriversStopListsViewModel : DialogTabViewModelBase
	{
		public DriversStopListsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			
		}
	}
}
