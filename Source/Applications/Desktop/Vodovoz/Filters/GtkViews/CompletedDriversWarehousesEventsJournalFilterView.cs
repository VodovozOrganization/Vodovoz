using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.Infrastructure.Converters;
using Gtk;

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
			eventEntry.ViewModel = ViewModel.DriverWarehouseEventViewModel;
			driverEntry.ViewModel = ViewModel.DriverViewModel;
			carEntry.ViewModel = ViewModel.CarViewModel;

			entryCompletedEventId.KeyReleaseEvent += UpdateFilter;
			entryCompletedEventId.Binding
				.AddBinding(ViewModel, vm => vm.CompletedEventId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			entryDistance.KeyReleaseEvent += UpdateFilter;
			entryDistance.Binding
				.AddBinding(ViewModel, vm => vm.DistanceFromScanning, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();

			enumCmbEventType.ShowSpecialStateAll = true;
			enumCmbEventType.ItemsEnum = typeof(DriverWarehouseEventType);
			enumCmbEventType.Binding
				.AddBinding(ViewModel, vm => vm.SelectedEventType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
		}

		private void UpdateFilter(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Return)
			{
				ViewModel.Update();
			}
		}
	}
}
