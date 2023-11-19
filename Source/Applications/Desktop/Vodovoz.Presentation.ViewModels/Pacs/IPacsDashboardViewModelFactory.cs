namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public interface IPacsDashboardViewModelFactory
	{
		DashboardOperatorOnBreakViewModel CreateOperatorOnBreakViewModel(OperatorModel operatorModel);
		DashboardOperatorViewModel CreateOperatorViewModel(OperatorModel operatorModel);
		DashboardMissedCallViewModel CreateMissedCallViewModel(MissedCallModel missedCallModel);
		DashboardCallViewModel CreateCallViewModel(CallModel callModel);
	}
}
