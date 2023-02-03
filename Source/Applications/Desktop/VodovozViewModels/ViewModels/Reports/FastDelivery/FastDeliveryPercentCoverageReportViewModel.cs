using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public class FastDeliveryPercentCoverageReportViewModel : DialogTabViewModelBase
	{
		public FastDeliveryPercentCoverageReportViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation) : base(unitOfWorkFactory, interactiveService, navigation)
		{
		}
	}
}
