using QS.ViewModels.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class IncomingCallsAnalysisReportViewModel : UowDialogViewModelBase
	{
		public IncomingCallsAnalysisReportViewModel(IUnitOfWorkFactory uowFactory, INavigationManager navigationManager)
			: base(uowFactory, navigationManager)
		{
		}
	}
}
