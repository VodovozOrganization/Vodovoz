using Vodovoz.Application.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public interface IPacsDashboardViewModelFactory
	{
		DashboardOperatorOnBreakViewModel CreateOperatorOnBreakViewModel(OperatorModel operatorModel);
		DashboardOperatorViewModel CreateOperatorViewModel(OperatorModel operatorModel);
		DashboardMissedCallViewModel CreateMissedCallViewModel(MissedCallModel missedCallModel);
		DashboardCallViewModel CreateCallViewModel(CallModel callModel);

		DashboardOperatorDetailsViewModel CreateOperatorDetailsViewModel(OperatorModel operatorModel);
		DashboardCallDetailsViewModel CreateCallDetailsViewModel(CallModel operatorModel);
		DashboardMissedCallDetailsViewModel CreateMissedCallDetailsViewModel(MissedCallModel operatorModel);
	}
}
