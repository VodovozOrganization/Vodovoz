using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Counterparties;
using static Vodovoz.ViewModels.Counterparties.CallTaskFilterViewModel;

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
			comboboxDateType.ItemsEnum = typeof(TaskFilterDateType);
			comboboxDateType.Binding.AddBinding(ViewModel, x => x.DateType, w => w.SelectedItem).InitializeFromSource();
			entVMEmployee.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeAutocompleteSelectorFactory);
			entVMEmployee.Binding.AddBinding(ViewModel, x => x.Employee, w => w.Subject).InitializeFromSource();
			checkbuttonHideCompleted.Binding.AddBinding(ViewModel, x => x.HideCompleted, w => w.Active).InitializeFromSource();
			showWithoutCheckButton.Binding.AddBinding(ViewModel, x => x.ShowOnlyWithoutEmployee, w => w.Active).InitializeFromSource();
			dateperiodpickerDateFilter.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			specialListCmbboxDlvPointCategory.ItemsList = ViewModel.ActiveDeliveryPointCategories;
			specialListCmbboxDlvPointCategory.Binding.AddBinding(ViewModel, x => x.DeliveryPointCategory, w => w.SelectedItem).InitializeFromSource();
			yenumcomboboxSortingParam.ItemsEnum = typeof(SortingParamType);
			yenumcomboboxSortingDirection.ItemsEnum = typeof(SortingDirectionType);
			yenumcomboboxSortingParam.Binding.AddBinding(ViewModel, x => x.SortingParam, w => w.SelectedItem).InitializeFromSource();
			yenumcomboboxSortingDirection.Binding.AddBinding(ViewModel, x => x.SortingDirection, w => w.SelectedItem).InitializeFromSource();

			ySpecCmbGeographicGroup.ItemsList = ViewModel.GeographicGroups;
			ySpecCmbGeographicGroup.Binding.AddBinding(ViewModel, vm => vm.GeographicGroup, w => w.SelectedItem).InitializeFromSource();

			buttonExpired.Clicked += (sender, e) => ViewModel.ChangeDateOnExpiredCommand.Execute();
			buttonToday.Clicked += (sender, e) => ViewModel.ChangeDateOnTodayCommand.Execute();
			buttonTomorrow.Clicked += (sender, e) => ViewModel.ChangeDateOnTomorrowCommand.Execute();
			buttonThisWeek.Clicked += (sender, e) => ViewModel.ChangeDateOnThisWeekCommand.Execute();
			buttonNextWeek.Clicked += (sender, e) => ViewModel.ChangeDateOnNextWeekCommand.Execute();
		}

		public override void Destroy()
		{
			comboboxDateType.Destroy();
			specialListCmbboxDlvPointCategory.ItemsList = null;
			specialListCmbboxDlvPointCategory.Destroy();
			yenumcomboboxSortingParam.Destroy();
			yenumcomboboxSortingDirection.Destroy();
			ySpecCmbGeographicGroup.ItemsList = null;
			ySpecCmbGeographicGroup.Destroy();
			base.Destroy();
		}
	}
}
