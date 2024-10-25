using System;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.Filters.GtkViews
{
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
			enumcomboCategory.ShowSpecialStateNot = false;
			
			if(ViewModel.HideEmployeeCategories != null && ViewModel.HideEmployeeCategories.Any())
			{
				enumcomboCategory.ShowSpecialStateAll = false;
				enumcomboCategory.AddEnumerableToHideList(ViewModel.HideEmployeeCategories);
			}
			else
			{
				enumcomboCategory.ShowSpecialStateAll = true;
			}
			
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
			checkSortByPriority.Toggled += UpdateViewModelRestrictions;

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			cmbDriverOfCarTypeOfUse.ItemsEnum = typeof(CarTypeOfUse);
			cmbDriverOfCarTypeOfUse.Binding
				.AddBinding(ViewModel, vm => vm.DriverOfCarTypeOfUse, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			cmbDriverOfCarOwnType.ItemsEnum = typeof(CarOwnType);
			cmbDriverOfCarOwnType.Binding
				.AddBinding(ViewModel, vm => vm.DriverOfCarOwnType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			registrationTypeCmb.ItemsEnum = typeof(RegistrationType);
			registrationTypeCmb.Binding
				.AddBinding(ViewModel, vm => vm.RegistrationType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			drpHiredDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HiredDatePeriodStart, w => w.StartDateOrNull)
				.AddBinding(vm => vm.HiredDatePeriodEnd, w => w.EndDateOrNull)
				.InitializeFromSource();

			drpFirstDayOnWork.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.FirstDayOnWorkStart, w => w.StartDateOrNull)
				.AddBinding(vm => vm.FirstDayOnWorkEnd, w => w.EndDateOrNull)
				.InitializeFromSource();

			drpFiredDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.FiredDatePeriodStart, w => w.StartDateOrNull)
				.AddBinding(vm => vm.FiredDatePeriodEnd, w => w.EndDateOrNull)
				.InitializeFromSource();

			drpSettlementDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SettlementDateStart, w => w.StartDateOrNull)
				.AddBinding(vm => vm.SettlementDateEnd, w => w.EndDateOrNull)
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
				.AddBinding(ViewModel, vm => vm.IsRFCitizen, w => w.Active)
				.InitializeFromSource();
		}

		private void UpdateViewModelRestrictions(object sender, EventArgs e)
		{
			ViewModel.UpdateRestrictions.Execute();
		}

		public override void Destroy()
		{
			checkSortByPriority.Toggled -= UpdateViewModelRestrictions;
			base.Destroy();
		}
	}
}
