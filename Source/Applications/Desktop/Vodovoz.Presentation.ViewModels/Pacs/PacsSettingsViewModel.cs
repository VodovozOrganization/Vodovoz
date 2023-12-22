using QS.Commands;
using QS.Navigation;
using QS.ViewModels;
using Vodovoz.Presentation.ViewModels.Employees.Journals;
using Vodovoz.Presentation.ViewModels.Pacs.Journals;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsSettingsViewModel : WidgetViewModelBase
	{
		private readonly IPacsViewModelOpener _pacsViewModelOpener;
		private readonly INavigationManager _navigationManager;
		public DelegateCommand OpenInnerPhonesReferenceBookCommand;
		public DelegateCommand OpenOperatorsReferenceBookCommand;
		public DelegateCommand OpenWorkShiftsReferenceBookCommand;

		public PacsSettingsViewModel(IPacsViewModelOpener pacsViewModelOpener, INavigationManager navigationManager, PacsDomainSettingsViewModel pacsDomainSettingsViewModel)
		{
			_pacsViewModelOpener = pacsViewModelOpener ?? throw new System.ArgumentNullException(nameof(pacsViewModelOpener));
			_navigationManager = navigationManager ?? throw new System.ArgumentNullException(nameof(navigationManager));
			DomainSettingsViewModel = pacsDomainSettingsViewModel ?? throw new System.ArgumentNullException(nameof(pacsDomainSettingsViewModel));
			OpenInnerPhonesReferenceBookCommand = new DelegateCommand(OpenInnerPhonesReferenceBook);
			OpenOperatorsReferenceBookCommand = new DelegateCommand(OpenOperatorsReferenceBook);
			OpenWorkShiftsReferenceBookCommand = new DelegateCommand(OpenWorkShiftsReferenceBook);
		}

		public PacsDomainSettingsViewModel DomainSettingsViewModel { get; }

		private void OpenInnerPhonesReferenceBook()
		{
			_navigationManager.OpenViewModel<InnerPhonesJournalViewModel>(null);
		}

		private void OpenOperatorsReferenceBook()
		{
			_pacsViewModelOpener.OpenOperatorsReferenceBook();
		}

		private void OpenWorkShiftsReferenceBook()
		{
			_navigationManager.OpenViewModel<WorkShiftJournalViewModel>(null);
		}
	}

	public interface IPacsViewModelOpener
	{
		void OpenOperatorsReferenceBook();
	}
}
