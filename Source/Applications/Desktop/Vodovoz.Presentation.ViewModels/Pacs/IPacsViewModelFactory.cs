namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public interface IPacsViewModelFactory
	{
		PacsDashboardViewModel CreateDashboardViewModel();
		PacsOperatorViewModel CreateOperatorViewModel();
		PacsReportsViewModel CreateReportsViewModel();
		PacsSettingsViewModel CreateSettingsViewModel();
	}
}
