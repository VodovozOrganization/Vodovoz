using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeFilterView : FilterViewBase<EmployeeFilterViewModel>
	{
		public EmployeeFilterView(EmployeeFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			enumcomboCategory.ItemsEnum = typeof(EmployeeCategory);
			enumcomboCategory.ShowSpecialStateAll = true;
			enumcomboCategory.ShowSpecialStateNot = false;

			enumcomboCategory.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Category, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.CanChangeCategory, w => w.Sensitive)
				.InitializeFromSource();

			yenumcomboStatus.ItemsEnum = typeof(EmployeeStatus);
			yenumcomboStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Status, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.CanChangeStatus, w => w.Sensitive)
				.InitializeFromSource();

			hboxDriversAndTerminals.Binding.AddBinding(ViewModel, vm => vm.HasAccessToDriverTerminal, w => w.Visible)
				.InitializeFromSource();

			comboDriverType.ItemsEnum = typeof(DriverTerminalRelation);
			comboDriverType.Binding.AddBinding(ViewModel, vm => vm.DriverTerminalRelation, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			checkSortByPriority.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanSortByPriority, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.SortByPriority, w => w.Active)
				.InitializeFromSource();
			checkSortByPriority.Toggled += (sender, args) => ViewModel.UpdateRestrictions.Execute();

			evmeSubdivision.Binding
				.AddBinding(ViewModel, vm => vm.Subdivision, w => w.Subject)
				.InitializeFromSource();

			cmbDriverOf.ItemsEnum = typeof(CarTypeOfUse);
			cmbDriverOf.Binding
				.AddBinding(ViewModel, vm => vm.DriverOf, w => w.SelectedItem)
				.InitializeFromSource();

			registrationTypeCmb.ItemsEnum = typeof(RegistrationType);
			registrationTypeCmb.Binding
				.AddBinding(ViewModel, vm => vm.RegistrationType, w => w.SelectedItem)
				.InitializeFromSource();

			drpHiredDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HiredDatePeriodStart, w => w.StartDate)
				.AddBinding(vm => vm.HiredDatePeriodEnd, w => w.EndDate)
				.InitializeFromSource();

			drpFirstDayOnWork.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.FirstDayOnWorkStart, w => w.StartDate)
				.AddBinding(vm => vm.FirstDayOnWorkEnd, w => w.EndDate)
				.InitializeFromSource();

			drpFiredDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.FiredDatePeriodStart, w => w.StartDate)
				.AddBinding(vm => vm.FiredDatePeriodEnd, w => w.EndDate)
				.InitializeFromSource();

			drpSettlementDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SettlementDateStart, w => w.StartDate)
				.AddBinding(vm => vm.SettlementDateEnd, w => w.EndDate)
				.InitializeFromSource();

			ychkVisitingMaster.Binding
				.AddBinding(ViewModel, vm => vm.IsVisitingMaster, w => w.Active)
				.InitializeFromSource();

			ychkDriverForOneDay.Binding
				.AddBinding(ViewModel, vm => vm.IsDriverForOneDay, w => w.Active)
				.InitializeFromSource();

			ychkChainStoreDriver.Binding
				.AddBinding(ViewModel, vm => vm.IsChainStoreDriver, w => w.Active)
				.InitializeFromSource();

			ychkRFcitizenship.Binding
				.AddBinding(ViewModel, vm => vm.IsRFcitizen, w => w.Active)
				.InitializeFromSource();
		}
	}
}
