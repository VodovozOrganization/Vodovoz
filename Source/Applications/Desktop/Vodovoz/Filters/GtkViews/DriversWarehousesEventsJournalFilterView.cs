using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.Infrastructure.Converters;

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
			entryEventId.KeyReleaseEvent += UpdateFilter;
			entryEventId.Binding
				.AddBinding(ViewModel, vm => vm.EventId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			entryEvent.KeyReleaseEvent += UpdateFilter;
			entryEvent.Binding
				.AddBinding(ViewModel, vm => vm.EventName, w => w.Text)
				.InitializeFromSource();

			entryLatitude.KeyReleaseEvent += UpdateFilter;
			entryLatitude.Binding
				.AddBinding(ViewModel, vm => vm.EventLatitude, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();

			entryLongitude.KeyReleaseEvent += UpdateFilter;
			entryLongitude.Binding
				.AddBinding(ViewModel, vm => vm.EventLongitude, w => w.Text, new NullableDecimalToStringConverter())
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
