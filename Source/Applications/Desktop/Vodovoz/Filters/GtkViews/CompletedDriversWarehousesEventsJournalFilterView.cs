using System;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.Infrastructure.Converters;
using Gtk;
using Vodovoz.Core.Domain.Logistics.Drivers;

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
			
			dateEventRangePicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			dateEventRangePicker.PeriodChangedByUser += OnDateEventPeriodChangedByUser;
		}

		private void OnDateEventPeriodChangedByUser(object sender, EventArgs e)
		{
			ViewModel.Update();
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
