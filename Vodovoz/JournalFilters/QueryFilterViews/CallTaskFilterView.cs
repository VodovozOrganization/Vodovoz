using System;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Tools;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters;
using Vodovoz.ViewModel;

namespace Vodovoz.JournalFilters.QueryFilterViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CallTaskFilterView : QueryFilterWidgetBase
	{
		public CallTaskFilter Filter { get; set; }

		public override IQueryFilter GetQueryFilter() => Filter;

		public CallTaskFilterView()
		{
			this.Build();
			Filter = new CallTaskFilter();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			EmployeesVM employeeVM = new EmployeesVM(UoW);
			comboboxDateType.ItemsEnum = typeof(TaskFilterDateType);
			comboboxDateType.Binding.AddBinding(Filter, x => x.DateType, w => w.SelectedItem).InitializeFromSource();
			employeeVM.Filter = new Filters.ViewModels.EmployeeFilterViewModel(QS.Project.Services.ServicesConfig.CommonServices);
			entryreferencevmEmployee.RepresentationModel = employeeVM;
			checkbuttonHideCompleted.Binding.AddBinding(Filter, x => x.HideCompleted, w => w.Active).InitializeFromSource();
			showWithoutCheckButton.Binding.AddBinding(Filter, x => x.ShowOnlyWihoutEmployee, w => w.Active).InitializeFromSource();
			dateperiodpickerDateFilter.Binding.AddBinding(Filter, x => x.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			showWithoutCheckButton.Binding.AddBinding(Filter, x => x.ShowOnlyWihoutEmployee, w => w.Active).InitializeFromSource();
			entryreferencevmEmployee.Binding.AddBinding(Filter, x => x.Employee, w => w.Subject).InitializeFromSource();
			dateperiodpickerDateFilter.Binding.AddBinding(Filter, x => x.EndDate, w => w.EndDateOrNull).InitializeFromSource();
			yenumcomboboxSortingParam.ItemsEnum = typeof(SortingParamType);
			yenumcomboboxSortingDirection.ItemsEnum = typeof(SortingDirectionType);
			yenumcomboboxSortingParam.Binding.AddBinding(Filter, x => x.SortingParam, w => w.SelectedItem).InitializeFromSource();
			yenumcomboboxSortingDirection.Binding.AddBinding(Filter, x => x.SortingDirection, w => w.SelectedItem).InitializeFromSource();
		}

		private void ChangeFilter(DateTime startDate , DateTime endDate)
		{
			Filter.SetDatePeriod(startDate, endDate);
			Refilter();
		}

		private void ChangeFilter(int weekIndex)
		{
			Filter.SetDatePeriod(weekIndex);
			Refilter();
		}

		#region EventHeandelers
		protected void OnCheckbuttonHideCompletedToggled(object sender, EventArgs e) => Refilter();

		protected void OnShowWithoutCheckButtonToggled(object sender, EventArgs e) => Refilter();

		protected void OnEntryreferencevmEmployeeChangedByUser(object sender, EventArgs e) => Refilter();

		protected void OnComboboxDateTypeChangedByUser(object sender, EventArgs e) => Refilter();

		protected void OnDateperiodpickerDateFilterPeriodChangedByUser(object sender, EventArgs e) => Refilter();

		protected void OnButtonExpiredClicked(object sender, EventArgs e) => ChangeFilter(DateTime.Now.AddDays(-15), DateTime.Now.AddDays(-1));

		protected void OnButtonTodayClicked(object sender, EventArgs e) => ChangeFilter(DateTime.Now, DateTime.Now);

		protected void OnButtonTomorrowClicked(object sender, EventArgs e) => ChangeFilter(DateTime.Now.AddDays(1), DateTime.Now.AddDays(1));

		protected void OnButtonThisWeekClicked(object sender, EventArgs e) => ChangeFilter(0);

		protected void OnButtonNextWeekClicked(object sender, EventArgs e) => ChangeFilter(1);

		protected void OnYenumcomboboxSortingDirectionChangedByUser(object sender, EventArgs e) => Refilter();

		protected void OnYenumcomboboxSortingParamChangedByUser(object sender, EventArgs e) => Refilter();

		protected void OnEntryreferencevmEmployeeChanged(object sender, EventArgs e)
		{
			showWithoutCheckButton.Visible = Filter.Employee == null;
		}
		#endregion EventHeandelers
	}
}
