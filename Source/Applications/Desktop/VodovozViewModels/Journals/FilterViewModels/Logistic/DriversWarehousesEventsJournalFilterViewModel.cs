using System;
using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class DriversWarehousesEventsJournalFilterViewModel : FilterViewModelBase<DriversWarehousesEventsJournalFilterViewModel>
	{
		private readonly DialogViewModelBase _journalViewModel;
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _scope;
		private DriverWarehouseEventName _eventName;
		private DriverWarehouseEventType? _selectedEventType;

		public DriversWarehousesEventsJournalFilterViewModel(
			DialogViewModelBase journalViewModel,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			Action<DriversWarehousesEventsJournalFilterViewModel> filterParameters = null)
		{
			_journalViewModel = journalViewModel ?? throw new ArgumentNullException(nameof(journalViewModel));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			ConfigureEntryViewModels();
			
			if(filterParameters != null)
			{
				SetAndRefilterAtOnce(filterParameters);
			}
		}
		
		public IEntityEntryViewModel DriverWarehouseEventNameViewModel { get; private set; }

		public DriverWarehouseEventName EventName
		{
			get => _eventName;
			set => UpdateFilterField(ref _eventName, value);
		}

		public DriverWarehouseEventType? SelectedEventType
		{
			get => _selectedEventType;
			set => UpdateFilterField(ref _selectedEventType, value);
		}
		
		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<DriversWarehousesEventsJournalFilterViewModel>(
				_journalViewModel, this, UoW, _navigationManager, _scope);

			DriverWarehouseEventNameViewModel =
				builder.ForProperty(x => x.EventName)
					.UseViewModelJournalAndAutocompleter<DriversWarehousesEventsNamesJournalViewModel>()
					.UseViewModelDialog<DriverWarehouseEventNameViewModel>()
					.Finish();
		}
	}
}
