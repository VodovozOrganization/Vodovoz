using QS.ViewModels;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsSettingsViewModel : WidgetViewModelBase
	{
		public PacsSettingsViewModel(PacsDomainSettingsViewModel pacsDomainSettingsViewModel)
		{
			DomainSettingsViewModel = pacsDomainSettingsViewModel ?? throw new System.ArgumentNullException(nameof(pacsDomainSettingsViewModel));
		}

		public PacsDomainSettingsViewModel DomainSettingsViewModel { get; }
	}
}
