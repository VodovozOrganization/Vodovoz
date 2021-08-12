using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeFilterView : FilterViewBase<EmployeeFilterViewModel>
	{
		public EmployeeFilterView(EmployeeFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
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
		}
	}
}
