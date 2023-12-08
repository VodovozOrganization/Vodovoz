using QS.Commands;
using QS.Navigation;
using QS.ViewModels;
using Vodovoz.Presentation.ViewModels.Employees;
using Vodovoz.Presentation.ViewModels.Employees.Journals;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsSettingsViewModel : WidgetViewModelBase
	{
		private readonly INavigationManager _navigationManager;
		public DelegateCommand OpenInnerPhonesReferenceBookCommand;

		public PacsSettingsViewModel(INavigationManager navigationManager, PacsDomainSettingsViewModel pacsDomainSettingsViewModel)
		{
			_navigationManager = navigationManager ?? throw new System.ArgumentNullException(nameof(navigationManager));
			DomainSettingsViewModel = pacsDomainSettingsViewModel ?? throw new System.ArgumentNullException(nameof(pacsDomainSettingsViewModel));
			OpenInnerPhonesReferenceBookCommand = new DelegateCommand(OpenInnerPhonesReferenceBook);
		}

		public PacsDomainSettingsViewModel DomainSettingsViewModel { get; }

		private void OpenInnerPhonesReferenceBook()
		{
			_navigationManager.OpenViewModel<InnerPhonesJournalViewModel>(null);
		}
	}
}
