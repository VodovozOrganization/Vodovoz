using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.Infrastructure.Converters;
using Gtk;
using Vodovoz.Core.Domain.Logistics.Drivers;
using QS.Widgets.GtkUI;
using Vodovoz.ReportsParameters;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CompletedDriversWarehousesEventsJournalFilterView :
		FilterViewBase<CompletedDriversWarehousesEventsJournalFilterViewModel>
	{
		private LeftRightListView _leftRightSortListView;
		
		public CompletedDriversWarehousesEventsJournalFilterView(
			CompletedDriversWarehousesEventsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			eventEntry.ViewModel = ViewModel.DriverWarehouseEventViewModel;
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
			
			chkOrderByEventDateDesc.Binding
				.AddBinding(ViewModel, vm => vm.OrderByEventDateDesc, w => w.Active)
				.InitializeFromSource();

			enumCmbDistanceCriterion.ItemsEnum = typeof(ComparisonSings);
			enumCmbDistanceCriterion.AddEnumToHideList(ComparisonSings.Equally, ComparisonSings.Less, ComparisonSings.More);
			enumCmbDistanceCriterion.Binding
				.AddBinding(ViewModel, vm => vm.DistanceCriterion, w => w.SelectedItem)
				.InitializeFromSource();

			CreateSelectableFilter();
			CreateAndConfigureGroupList();
		}

		private void CreateSelectableFilter()
		{
			var filterWidget = new SelectableParameterReportFilterView(ViewModel.SelectableParameterReportFilterViewModel);
			vboxDrivers.Add(filterWidget);
			
			var boxFilter = (Box.BoxChild)hboxMain[vboxDrivers];
			boxFilter.Expand = false;
			
			filterWidget.Show();
		}
		
		private void CreateAndConfigureGroupList()
		{
			_leftRightSortListView = new LeftRightListView();
			_leftRightSortListView.ViewModel = ViewModel.SortViewModel;
			hboxMain.Add(_leftRightSortListView);
			
			var boxListView = (Box.BoxChild)hboxMain[_leftRightSortListView];
			boxListView.Expand = false;
			
			_leftRightSortListView.Show();
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
