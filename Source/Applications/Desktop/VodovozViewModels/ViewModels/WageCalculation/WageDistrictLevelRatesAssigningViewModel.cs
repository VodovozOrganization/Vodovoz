using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;

namespace Vodovoz.ViewModels.ViewModels.WageCalculation
{
	public class WageDistrictLevelRatesAssigningViewModel : DialogTabViewModelBase
	{
		public WageDistrictLevelRatesAssigningViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			Title = "Привязка ставок";
		}
	}
}
