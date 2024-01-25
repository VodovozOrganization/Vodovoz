using System;
using System.Linq;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.Infrastructure.Converters;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Project.DB;
using QS.Widgets.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CompletedDriversWarehousesEventsJournalFilterView :
		FilterViewBase<CompletedDriversWarehousesEventsJournalFilterViewModel>
	{
		private readonly SelectableParametersReportFilter _filter;
		private LeftRightListView _leftRightSortListView;
		
		public CompletedDriversWarehousesEventsJournalFilterView(
			CompletedDriversWarehousesEventsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			_filter = new SelectableParametersReportFilter(ViewModel.UoW);
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

			ConfigureSelectableFilter();
			CreateAndConfigureGroupList();
		}

		private void ConfigureSelectableFilter()
		{
			_filter.CreateParameterSet(
				"Водители",
				"drivers",
				new ParametersFactory(ViewModel.UoW, (filters) =>
				{
					SelectableEntityParameter<Employee> resultAlias = null;
					var query = ViewModel.UoW.Session.QueryOver<Employee>();

					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					var employeeProjection = CustomProjections.Concat_WS(
						" ",
						Projections.Property<Employee>(x => x.LastName),
						Projections.Property<Employee>(x => x.Name),
						Projections.Property<Employee>(x => x.Patronymic)
					);

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(employeeProjection).WithAlias(() => resultAlias.EntityTitle))
						.OrderBy(x => x.LastName).Asc
						.OrderBy(x => x.Name).Asc
						.OrderBy(x => x.Patronymic).Asc
						.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
					
					return query.List<SelectableParameter>();
				})
			);

			var viewModel = new SelectableParameterReportFilterViewModel(_filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
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
