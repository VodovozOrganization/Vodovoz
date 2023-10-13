using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CompletedDriversWarehousesEventsJournalFilterView :
		FilterViewBase<CompletedDriversWarehousesEventsJournalFilterViewModel>
	{
		public CompletedDriversWarehousesEventsJournalFilterView(
			CompletedDriversWarehousesEventsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			eventNameEntry.ViewModel = ViewModel.DriverWarehouseEventNameViewModel;
			driverEntry.ViewModel = ViewModel.DriverViewModel;
			carEntry.ViewModel = ViewModel.CarViewModel;

			enumCmbEventType.ShowSpecialStateAll = true;
			enumCmbEventType.ItemsEnum = typeof(DriverWarehouseEventType);
			enumCmbEventType.Binding
				.AddBinding(ViewModel, vm => vm.SelectedEventType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
		}
	}
}
