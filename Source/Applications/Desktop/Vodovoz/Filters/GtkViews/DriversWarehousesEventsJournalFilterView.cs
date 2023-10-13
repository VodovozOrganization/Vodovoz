using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriversWarehousesEventsJournalFilterView : FilterViewBase<DriversWarehousesEventsJournalFilterViewModel>
	{
		public DriversWarehousesEventsJournalFilterView(DriversWarehousesEventsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			eventNameEntry.ViewModel = ViewModel.DriverWarehouseEventNameViewModel;

			enumCmbEventType.ShowSpecialStateAll = true;
			enumCmbEventType.ItemsEnum = typeof(DriverWarehouseEventType);
			enumCmbEventType.Binding
				.AddBinding(ViewModel, vm => vm.SelectedEventType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
		}
	}
}
