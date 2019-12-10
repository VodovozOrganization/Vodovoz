using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CallTaskFilterView : FilterViewBase<CallTaskFilterViewModel>
	{
		public CallTaskFilterView(CallTaskFilterViewModel callTaskFilterViewModel) : base(callTaskFilterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			var UoW = UnitOfWorkFactory.CreateWithoutRoot();

			comboboxDateType.ItemsEnum = typeof(TaskFilterDateType);
			comboboxDateType.Binding.AddBinding(ViewModel, x => x.DateType, w => w.SelectedItem).InitializeFromSource();
			entVMEmployee.SetEntityAutocompleteSelectorFactory(new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices));
			entVMEmployee.Binding.AddBinding(ViewModel, x => x.Employee, w => w.Subject).InitializeFromSource();
			checkbuttonHideCompleted.Binding.AddBinding(ViewModel, x => x.HideCompleted, w => w.Active).InitializeFromSource();
			showWithoutCheckButton.Binding.AddBinding(ViewModel, x => x.ShowOnlyWithoutEmployee, w => w.Active).InitializeFromSource();
			dateperiodpickerDateFilter.Binding.AddBinding(ViewModel, x => x.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			dateperiodpickerDateFilter.Binding.AddBinding(ViewModel, x => x.EndDate, w => w.EndDateOrNull).InitializeFromSource();
			specialListCmbboxDlvPointCategory.ItemsList = UoW.Session.QueryOver<DeliveryPointCategory>().Where(c => !c.IsArchive).List().OrderBy(c => c.Name);
			specialListCmbboxDlvPointCategory.Binding.AddBinding(ViewModel, x => x.DeliveryPointCategory, w => w.SelectedItem).InitializeFromSource();
			yenumcomboboxSortingParam.ItemsEnum = typeof(SortingParamType);
			yenumcomboboxSortingDirection.ItemsEnum = typeof(SortingDirectionType);
			yenumcomboboxSortingParam.Binding.AddBinding(ViewModel, x => x.SortingParam, w => w.SelectedItem).InitializeFromSource();
			yenumcomboboxSortingDirection.Binding.AddBinding(ViewModel, x => x.SortingDirection, w => w.SelectedItem).InitializeFromSource();

			buttonExpired.Clicked += (sender, e) => ViewModel.ChangeDateOnExpiredCommand.Execute();
			buttonToday.Clicked += (sender, e) => ViewModel.ChangeDateOnTodayCommand.Execute();
			buttonTomorrow.Clicked += (sender, e) => ViewModel.ChangeDateOnTomorrowCommand.Execute();
			buttonThisWeek.Clicked += (sender, e) => ViewModel.ChangeDateOnThisWeekCommand.Execute();
			buttonNextWeek.Clicked += (sender, e) => ViewModel.ChangeDateOnNextWeekCommand.Execute();
		}
	}
}
